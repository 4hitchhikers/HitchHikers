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
import { AbortController } from "./AbortController";
import { HttpError, TimeoutError } from "./Errors";
import { LogLevel } from "./ILogger";
import { Arg } from "./Utils";
export var TransportType;
(function (TransportType) {
    TransportType[TransportType["WebSockets"] = 0] = "WebSockets";
    TransportType[TransportType["ServerSentEvents"] = 1] = "ServerSentEvents";
    TransportType[TransportType["LongPolling"] = 2] = "LongPolling";
})(TransportType || (TransportType = {}));
export var TransferFormat;
(function (TransferFormat) {
    TransferFormat[TransferFormat["Text"] = 1] = "Text";
    TransferFormat[TransferFormat["Binary"] = 2] = "Binary";
})(TransferFormat || (TransferFormat = {}));
export class WebSocketTransport {
    constructor(accessTokenFactory, logger) {
        this.logger = logger;
        this.accessTokenFactory = accessTokenFactory || (() => null);
    }
    connect(url, transferFormat, connection) {
        Arg.isRequired(url, "url");
        Arg.isRequired(transferFormat, "transferFormat");
        Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        Arg.isRequired(connection, "connection");
        if (typeof (WebSocket) === "undefined") {
            throw new Error("'WebSocket' is not supported in your environment.");
        }
        this.logger.log(LogLevel.Trace, "(WebSockets transport) Connecting");
        return new Promise((resolve, reject) => {
            url = url.replace(/^http/, "ws");
            const token = this.accessTokenFactory();
            if (token) {
                url += (url.indexOf("?") < 0 ? "?" : "&") + `access_token=${encodeURIComponent(token)}`;
            }
            const webSocket = new WebSocket(url);
            if (transferFormat === TransferFormat.Binary) {
                webSocket.binaryType = "arraybuffer";
            }
            webSocket.onopen = (event) => {
                this.logger.log(LogLevel.Information, `WebSocket connected to ${url}`);
                this.webSocket = webSocket;
                resolve();
            };
            webSocket.onerror = (event) => {
                reject(event.error);
            };
            webSocket.onmessage = (message) => {
                this.logger.log(LogLevel.Trace, `(WebSockets transport) data received. ${getDataDetail(message.data)}.`);
                if (this.onreceive) {
                    this.onreceive(message.data);
                }
            };
            webSocket.onclose = (event) => {
                // webSocket will be null if the transport did not start successfully
                if (this.onclose && this.webSocket) {
                    if (event.wasClean === false || event.code !== 1000) {
                        this.onclose(new Error(`Websocket closed with status code: ${event.code} (${event.reason})`));
                    }
                    else {
                        this.onclose();
                    }
                }
            };
        });
    }
    send(data) {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.logger.log(LogLevel.Trace, `(WebSockets transport) sending data. ${getDataDetail(data)}.`);
            this.webSocket.send(data);
            return Promise.resolve();
        }
        return Promise.reject("WebSocket is not in the OPEN state");
    }
    stop() {
        if (this.webSocket) {
            this.webSocket.close();
            this.webSocket = null;
        }
        return Promise.resolve();
    }
}
export class ServerSentEventsTransport {
    constructor(httpClient, accessTokenFactory, logger) {
        this.httpClient = httpClient;
        this.accessTokenFactory = accessTokenFactory || (() => null);
        this.logger = logger;
    }
    connect(url, transferFormat, connection) {
        Arg.isRequired(url, "url");
        Arg.isRequired(transferFormat, "transferFormat");
        Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        Arg.isRequired(connection, "connection");
        if (typeof (EventSource) === "undefined") {
            throw new Error("'EventSource' is not supported in your environment.");
        }
        this.logger.log(LogLevel.Trace, "(SSE transport) Connecting");
        this.url = url;
        return new Promise((resolve, reject) => {
            if (transferFormat !== TransferFormat.Text) {
                reject(new Error("The Server-Sent Events transport only supports the 'Text' transfer format"));
            }
            const token = this.accessTokenFactory();
            if (token) {
                url += (url.indexOf("?") < 0 ? "?" : "&") + `access_token=${encodeURIComponent(token)}`;
            }
            const eventSource = new EventSource(url, { withCredentials: true });
            try {
                eventSource.onmessage = (e) => {
                    if (this.onreceive) {
                        try {
                            this.logger.log(LogLevel.Trace, `(SSE transport) data received. ${getDataDetail(e.data)}.`);
                            this.onreceive(e.data);
                        }
                        catch (error) {
                            if (this.onclose) {
                                this.onclose(error);
                            }
                            return;
                        }
                    }
                };
                eventSource.onerror = (e) => {
                    reject(new Error(e.message || "Error occurred"));
                    // don't report an error if the transport did not start successfully
                    if (this.eventSource && this.onclose) {
                        this.onclose(new Error(e.message || "Error occurred"));
                    }
                };
                eventSource.onopen = () => {
                    this.logger.log(LogLevel.Information, `SSE connected to ${this.url}`);
                    this.eventSource = eventSource;
                    // SSE is a text protocol
                    resolve();
                };
            }
            catch (e) {
                return Promise.reject(e);
            }
        });
    }
    send(data) {
        return __awaiter(this, void 0, void 0, function* () {
            return send(this.logger, "SSE", this.httpClient, this.url, this.accessTokenFactory, data);
        });
    }
    stop() {
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }
        return Promise.resolve();
    }
}
export class LongPollingTransport {
    constructor(httpClient, accessTokenFactory, logger) {
        this.httpClient = httpClient;
        this.accessTokenFactory = accessTokenFactory || (() => null);
        this.logger = logger;
        this.pollAbort = new AbortController();
    }
    connect(url, transferFormat, connection) {
        Arg.isRequired(url, "url");
        Arg.isRequired(transferFormat, "transferFormat");
        Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        Arg.isRequired(connection, "connection");
        this.url = url;
        this.logger.log(LogLevel.Trace, "(LongPolling transport) Connecting");
        // Set a flag indicating we have inherent keep-alive in this transport.
        connection.features.inherentKeepAlive = true;
        if (transferFormat === TransferFormat.Binary && (typeof new XMLHttpRequest().responseType !== "string")) {
            // This will work if we fix: https://github.com/aspnet/SignalR/issues/742
            throw new Error("Binary protocols over XmlHttpRequest not implementing advanced features are not supported.");
        }
        this.poll(this.url, transferFormat);
        return Promise.resolve();
    }
    poll(url, transferFormat) {
        return __awaiter(this, void 0, void 0, function* () {
            const pollOptions = {
                abortSignal: this.pollAbort.signal,
                headers: {},
                timeout: 90000,
            };
            if (transferFormat === TransferFormat.Binary) {
                pollOptions.responseType = "arraybuffer";
            }
            const token = this.accessTokenFactory();
            if (token) {
                // tslint:disable-next-line:no-string-literal
                pollOptions.headers["Authorization"] = `Bearer ${token}`;
            }
            while (!this.pollAbort.signal.aborted) {
                try {
                    const pollUrl = `${url}&_=${Date.now()}`;
                    this.logger.log(LogLevel.Trace, `(LongPolling transport) polling: ${pollUrl}`);
                    const response = yield this.httpClient.get(pollUrl, pollOptions);
                    if (response.statusCode === 204) {
                        this.logger.log(LogLevel.Information, "(LongPolling transport) Poll terminated by server");
                        // Poll terminated by server
                        if (this.onclose) {
                            this.onclose();
                        }
                        this.pollAbort.abort();
                    }
                    else if (response.statusCode !== 200) {
                        this.logger.log(LogLevel.Error, `(LongPolling transport) Unexpected response code: ${response.statusCode}`);
                        // Unexpected status code
                        if (this.onclose) {
                            this.onclose(new HttpError(response.statusText, response.statusCode));
                        }
                        this.pollAbort.abort();
                    }
                    else {
                        // Process the response
                        if (response.content) {
                            this.logger.log(LogLevel.Trace, `(LongPolling transport) data received. ${getDataDetail(response.content)}.`);
                            if (this.onreceive) {
                                this.onreceive(response.content);
                            }
                        }
                        else {
                            // This is another way timeout manifest.
                            this.logger.log(LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                        }
                    }
                }
                catch (e) {
                    if (e instanceof TimeoutError) {
                        // Ignore timeouts and reissue the poll.
                        this.logger.log(LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                    }
                    else {
                        // Close the connection with the error as the result.
                        if (this.onclose) {
                            this.onclose(e);
                        }
                        this.pollAbort.abort();
                    }
                }
            }
        });
    }
    send(data) {
        return __awaiter(this, void 0, void 0, function* () {
            return send(this.logger, "LongPolling", this.httpClient, this.url, this.accessTokenFactory, data);
        });
    }
    stop() {
        this.pollAbort.abort();
        return Promise.resolve();
    }
}
function getDataDetail(data) {
    let length = null;
    if (data instanceof ArrayBuffer) {
        length = `Binary data of length ${data.byteLength}`;
    }
    else if (typeof data === "string") {
        length = `String data of length ${data.length}`;
    }
    return length;
}
function send(logger, transportName, httpClient, url, accessTokenFactory, content) {
    return __awaiter(this, void 0, void 0, function* () {
        let headers;
        const token = accessTokenFactory();
        if (token) {
            headers = {
                ["Authorization"]: `Bearer ${accessTokenFactory()}`,
            };
        }
        logger.log(LogLevel.Trace, `(${transportName} transport) sending data. ${getDataDetail(content)}.`);
        const response = yield httpClient.post(url, {
            content,
            headers,
        });
        logger.log(LogLevel.Trace, `(${transportName} transport) request complete. Response status: ${response.statusCode}.`);
    });
}
//# sourceMappingURL=Transports.js.map