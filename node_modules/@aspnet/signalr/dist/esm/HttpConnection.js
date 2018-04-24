// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import { DefaultHttpClient } from "./HttpClient";
import { LogLevel } from "./ILogger";
import { LoggerFactory } from "./Loggers";
import { LongPollingTransport, ServerSentEventsTransport, TransferFormat, TransportType, WebSocketTransport } from "./Transports";
import { Arg } from "./Utils";
export class HttpConnection {
    constructor(url, options = {}) {
        this.features = {};
        Arg.isRequired(url, "url");
        this.logger = LoggerFactory.createLogger(options.logger);
        this.baseUrl = this.resolveUrl(url);
        options = options || {};
        options.accessTokenFactory = options.accessTokenFactory || (() => null);
        this.httpClient = options.httpClient || new DefaultHttpClient(this.logger);
        this.connectionState = 2 /* Disconnected */;
        this.options = options;
    }
    start(transferFormat) {
        Arg.isRequired(transferFormat, "transferFormat");
        Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        this.logger.log(LogLevel.Trace, `Starting connection with transfer format '${TransferFormat[transferFormat]}'.`);
        if (this.connectionState !== 2 /* Disconnected */) {
            return Promise.reject(new Error("Cannot start a connection that is not in the 'Disconnected' state."));
        }
        this.connectionState = 0 /* Connecting */;
        this.startPromise = this.startInternal(transferFormat);
        return this.startPromise;
    }
    startInternal(transferFormat) {
        return __awaiter(this, void 0, void 0, function* () {
            try {
                if (this.options.transport === TransportType.WebSockets) {
                    // No need to add a connection ID in this case
                    this.url = this.baseUrl;
                    this.transport = this.constructTransport(TransportType.WebSockets);
                    // We should just call connect directly in this case.
                    // No fallback or negotiate in this case.
                    yield this.transport.connect(this.url, transferFormat, this);
                }
                else {
                    const token = this.options.accessTokenFactory();
                    let headers;
                    if (token) {
                        headers = {
                            ["Authorization"]: `Bearer ${token}`,
                        };
                    }
                    const negotiateResponse = yield this.getNegotiationResponse(headers);
                    // the user tries to stop the the connection when it is being started
                    if (this.connectionState === 2 /* Disconnected */) {
                        return;
                    }
                    yield this.createTransport(this.options.transport, negotiateResponse, transferFormat, headers);
                }
                this.transport.onreceive = this.onreceive;
                this.transport.onclose = (e) => this.stopConnection(true, e);
                // only change the state if we were connecting to not overwrite
                // the state if the connection is already marked as Disconnected
                this.changeState(0 /* Connecting */, 1 /* Connected */);
            }
            catch (e) {
                this.logger.log(LogLevel.Error, "Failed to start the connection: " + e);
                this.connectionState = 2 /* Disconnected */;
                this.transport = null;
                throw e;
            }
        });
    }
    getNegotiationResponse(headers) {
        return __awaiter(this, void 0, void 0, function* () {
            const negotiateUrl = this.resolveNegotiateUrl(this.baseUrl);
            this.logger.log(LogLevel.Trace, `Sending negotiation request: ${negotiateUrl}`);
            try {
                const response = yield this.httpClient.post(negotiateUrl, {
                    content: "",
                    headers,
                });
                return JSON.parse(response.content);
            }
            catch (e) {
                this.logger.log(LogLevel.Error, "Failed to complete negotiation with the server: " + e);
                throw e;
            }
        });
    }
    updateConnectionId(negotiateResponse) {
        this.connectionId = negotiateResponse.connectionId;
        this.url = this.baseUrl + (this.baseUrl.indexOf("?") === -1 ? "?" : "&") + `id=${this.connectionId}`;
    }
    createTransport(requestedTransport, negotiateResponse, requestedTransferFormat, headers) {
        return __awaiter(this, void 0, void 0, function* () {
            this.updateConnectionId(negotiateResponse);
            if (this.isITransport(requestedTransport)) {
                this.logger.log(LogLevel.Trace, "Connection was provided an instance of ITransport, using that directly.");
                this.transport = requestedTransport;
                yield this.transport.connect(this.url, requestedTransferFormat, this);
                // only change the state if we were connecting to not overwrite
                // the state if the connection is already marked as Disconnected
                this.changeState(0 /* Connecting */, 1 /* Connected */);
                return;
            }
            const transports = negotiateResponse.availableTransports;
            for (const endpoint of transports) {
                this.connectionState = 0 /* Connecting */;
                const transport = this.resolveTransport(endpoint, requestedTransport, requestedTransferFormat);
                if (typeof transport === "number") {
                    this.transport = this.constructTransport(transport);
                    if (negotiateResponse.connectionId === null) {
                        negotiateResponse = yield this.getNegotiationResponse(headers);
                        this.updateConnectionId(negotiateResponse);
                    }
                    try {
                        yield this.transport.connect(this.url, requestedTransferFormat, this);
                        this.changeState(0 /* Connecting */, 1 /* Connected */);
                        return;
                    }
                    catch (ex) {
                        this.logger.log(LogLevel.Error, `Failed to start the transport '${TransportType[transport]}': ${ex}`);
                        this.connectionState = 2 /* Disconnected */;
                        negotiateResponse.connectionId = null;
                    }
                }
            }
            throw new Error("Unable to initialize any of the available transports.");
        });
    }
    constructTransport(transport) {
        switch (transport) {
            case TransportType.WebSockets:
                return new WebSocketTransport(this.options.accessTokenFactory, this.logger);
            case TransportType.ServerSentEvents:
                return new ServerSentEventsTransport(this.httpClient, this.options.accessTokenFactory, this.logger);
            case TransportType.LongPolling:
                return new LongPollingTransport(this.httpClient, this.options.accessTokenFactory, this.logger);
            default:
                throw new Error(`Unknown transport: ${transport}.`);
        }
    }
    resolveTransport(endpoint, requestedTransport, requestedTransferFormat) {
        const transport = TransportType[endpoint.transport];
        if (transport === null || transport === undefined) {
            this.logger.log(LogLevel.Trace, `Skipping transport '${endpoint.transport}' because it is not supported by this client.`);
        }
        else {
            const transferFormats = endpoint.transferFormats.map((s) => TransferFormat[s]);
            if (!requestedTransport || transport === requestedTransport) {
                if (transferFormats.indexOf(requestedTransferFormat) >= 0) {
                    if ((transport === TransportType.WebSockets && typeof WebSocket === "undefined") ||
                        (transport === TransportType.ServerSentEvents && typeof EventSource === "undefined")) {
                        this.logger.log(LogLevel.Trace, `Skipping transport '${TransportType[transport]}' because it is not supported in your environment.'`);
                    }
                    else {
                        this.logger.log(LogLevel.Trace, `Selecting transport '${TransportType[transport]}'`);
                        return transport;
                    }
                }
                else {
                    this.logger.log(LogLevel.Trace, `Skipping transport '${TransportType[transport]}' because it does not support the requested transfer format '${TransferFormat[requestedTransferFormat]}'.`);
                }
            }
            else {
                this.logger.log(LogLevel.Trace, `Skipping transport '${TransportType[transport]}' because it was disabled by the client.`);
            }
        }
        return null;
    }
    isITransport(transport) {
        return typeof (transport) === "object" && "connect" in transport;
    }
    changeState(from, to) {
        if (this.connectionState === from) {
            this.connectionState = to;
            return true;
        }
        return false;
    }
    send(data) {
        if (this.connectionState !== 1 /* Connected */) {
            throw new Error("Cannot send data if the connection is not in the 'Connected' State.");
        }
        return this.transport.send(data);
    }
    stop(error) {
        return __awaiter(this, void 0, void 0, function* () {
            const previousState = this.connectionState;
            this.connectionState = 2 /* Disconnected */;
            try {
                yield this.startPromise;
            }
            catch (e) {
                // this exception is returned to the user as a rejected Promise from the start method
            }
            this.stopConnection(/*raiseClosed*/ previousState === 1 /* Connected */, error);
        });
    }
    stopConnection(raiseClosed, error) {
        if (this.transport) {
            this.transport.stop();
            this.transport = null;
        }
        if (error) {
            this.logger.log(LogLevel.Error, `Connection disconnected with error '${error}'.`);
        }
        else {
            this.logger.log(LogLevel.Information, "Connection disconnected.");
        }
        this.connectionState = 2 /* Disconnected */;
        if (raiseClosed && this.onclose) {
            this.onclose(error);
        }
    }
    resolveUrl(url) {
        // startsWith is not supported in IE
        if (url.lastIndexOf("https://", 0) === 0 || url.lastIndexOf("http://", 0) === 0) {
            return url;
        }
        if (typeof window === "undefined" || !window || !window.document) {
            throw new Error(`Cannot resolve '${url}'.`);
        }
        const parser = window.document.createElement("a");
        parser.href = url;
        const baseUrl = (!parser.protocol || parser.protocol === ":")
            ? `${window.document.location.protocol}//${(parser.host || window.document.location.host)}`
            : `${parser.protocol}//${parser.host}`;
        if (!url || url[0] !== "/") {
            url = "/" + url;
        }
        const normalizedUrl = baseUrl + url;
        this.logger.log(LogLevel.Information, `Normalizing '${url}' to '${normalizedUrl}'.`);
        return normalizedUrl;
    }
    resolveNegotiateUrl(url) {
        const index = url.indexOf("?");
        let negotiateUrl = url.substring(0, index === -1 ? url.length : index);
        if (negotiateUrl[negotiateUrl.length - 1] !== "/") {
            negotiateUrl += "/";
        }
        negotiateUrl += "negotiate";
        negotiateUrl += index === -1 ? "" : url.substring(index);
        return negotiateUrl;
    }
}
//# sourceMappingURL=HttpConnection.js.map