"use strict";
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
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var AbortController_1 = require("./AbortController");
var Errors_1 = require("./Errors");
var ILogger_1 = require("./ILogger");
var Utils_1 = require("./Utils");
var TransportType;
(function (TransportType) {
    TransportType[TransportType["WebSockets"] = 0] = "WebSockets";
    TransportType[TransportType["ServerSentEvents"] = 1] = "ServerSentEvents";
    TransportType[TransportType["LongPolling"] = 2] = "LongPolling";
})(TransportType = exports.TransportType || (exports.TransportType = {}));
var TransferFormat;
(function (TransferFormat) {
    TransferFormat[TransferFormat["Text"] = 1] = "Text";
    TransferFormat[TransferFormat["Binary"] = 2] = "Binary";
})(TransferFormat = exports.TransferFormat || (exports.TransferFormat = {}));
var WebSocketTransport = /** @class */ (function () {
    function WebSocketTransport(accessTokenFactory, logger) {
        this.logger = logger;
        this.accessTokenFactory = accessTokenFactory || (function () { return null; });
    }
    WebSocketTransport.prototype.connect = function (url, transferFormat, connection) {
        var _this = this;
        Utils_1.Arg.isRequired(url, "url");
        Utils_1.Arg.isRequired(transferFormat, "transferFormat");
        Utils_1.Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        Utils_1.Arg.isRequired(connection, "connection");
        if (typeof (WebSocket) === "undefined") {
            throw new Error("'WebSocket' is not supported in your environment.");
        }
        this.logger.log(ILogger_1.LogLevel.Trace, "(WebSockets transport) Connecting");
        return new Promise(function (resolve, reject) {
            url = url.replace(/^http/, "ws");
            var token = _this.accessTokenFactory();
            if (token) {
                url += (url.indexOf("?") < 0 ? "?" : "&") + ("access_token=" + encodeURIComponent(token));
            }
            var webSocket = new WebSocket(url);
            if (transferFormat === TransferFormat.Binary) {
                webSocket.binaryType = "arraybuffer";
            }
            webSocket.onopen = function (event) {
                _this.logger.log(ILogger_1.LogLevel.Information, "WebSocket connected to " + url);
                _this.webSocket = webSocket;
                resolve();
            };
            webSocket.onerror = function (event) {
                reject(event.error);
            };
            webSocket.onmessage = function (message) {
                _this.logger.log(ILogger_1.LogLevel.Trace, "(WebSockets transport) data received. " + getDataDetail(message.data) + ".");
                if (_this.onreceive) {
                    _this.onreceive(message.data);
                }
            };
            webSocket.onclose = function (event) {
                // webSocket will be null if the transport did not start successfully
                if (_this.onclose && _this.webSocket) {
                    if (event.wasClean === false || event.code !== 1000) {
                        _this.onclose(new Error("Websocket closed with status code: " + event.code + " (" + event.reason + ")"));
                    }
                    else {
                        _this.onclose();
                    }
                }
            };
        });
    };
    WebSocketTransport.prototype.send = function (data) {
        if (this.webSocket && this.webSocket.readyState === WebSocket.OPEN) {
            this.logger.log(ILogger_1.LogLevel.Trace, "(WebSockets transport) sending data. " + getDataDetail(data) + ".");
            this.webSocket.send(data);
            return Promise.resolve();
        }
        return Promise.reject("WebSocket is not in the OPEN state");
    };
    WebSocketTransport.prototype.stop = function () {
        if (this.webSocket) {
            this.webSocket.close();
            this.webSocket = null;
        }
        return Promise.resolve();
    };
    return WebSocketTransport;
}());
exports.WebSocketTransport = WebSocketTransport;
var ServerSentEventsTransport = /** @class */ (function () {
    function ServerSentEventsTransport(httpClient, accessTokenFactory, logger) {
        this.httpClient = httpClient;
        this.accessTokenFactory = accessTokenFactory || (function () { return null; });
        this.logger = logger;
    }
    ServerSentEventsTransport.prototype.connect = function (url, transferFormat, connection) {
        var _this = this;
        Utils_1.Arg.isRequired(url, "url");
        Utils_1.Arg.isRequired(transferFormat, "transferFormat");
        Utils_1.Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        Utils_1.Arg.isRequired(connection, "connection");
        if (typeof (EventSource) === "undefined") {
            throw new Error("'EventSource' is not supported in your environment.");
        }
        this.logger.log(ILogger_1.LogLevel.Trace, "(SSE transport) Connecting");
        this.url = url;
        return new Promise(function (resolve, reject) {
            if (transferFormat !== TransferFormat.Text) {
                reject(new Error("The Server-Sent Events transport only supports the 'Text' transfer format"));
            }
            var token = _this.accessTokenFactory();
            if (token) {
                url += (url.indexOf("?") < 0 ? "?" : "&") + ("access_token=" + encodeURIComponent(token));
            }
            var eventSource = new EventSource(url, { withCredentials: true });
            try {
                eventSource.onmessage = function (e) {
                    if (_this.onreceive) {
                        try {
                            _this.logger.log(ILogger_1.LogLevel.Trace, "(SSE transport) data received. " + getDataDetail(e.data) + ".");
                            _this.onreceive(e.data);
                        }
                        catch (error) {
                            if (_this.onclose) {
                                _this.onclose(error);
                            }
                            return;
                        }
                    }
                };
                eventSource.onerror = function (e) {
                    reject(new Error(e.message || "Error occurred"));
                    // don't report an error if the transport did not start successfully
                    if (_this.eventSource && _this.onclose) {
                        _this.onclose(new Error(e.message || "Error occurred"));
                    }
                };
                eventSource.onopen = function () {
                    _this.logger.log(ILogger_1.LogLevel.Information, "SSE connected to " + _this.url);
                    _this.eventSource = eventSource;
                    // SSE is a text protocol
                    resolve();
                };
            }
            catch (e) {
                return Promise.reject(e);
            }
        });
    };
    ServerSentEventsTransport.prototype.send = function (data) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                return [2 /*return*/, send(this.logger, "SSE", this.httpClient, this.url, this.accessTokenFactory, data)];
            });
        });
    };
    ServerSentEventsTransport.prototype.stop = function () {
        if (this.eventSource) {
            this.eventSource.close();
            this.eventSource = null;
        }
        return Promise.resolve();
    };
    return ServerSentEventsTransport;
}());
exports.ServerSentEventsTransport = ServerSentEventsTransport;
var LongPollingTransport = /** @class */ (function () {
    function LongPollingTransport(httpClient, accessTokenFactory, logger) {
        this.httpClient = httpClient;
        this.accessTokenFactory = accessTokenFactory || (function () { return null; });
        this.logger = logger;
        this.pollAbort = new AbortController_1.AbortController();
    }
    LongPollingTransport.prototype.connect = function (url, transferFormat, connection) {
        Utils_1.Arg.isRequired(url, "url");
        Utils_1.Arg.isRequired(transferFormat, "transferFormat");
        Utils_1.Arg.isIn(transferFormat, TransferFormat, "transferFormat");
        Utils_1.Arg.isRequired(connection, "connection");
        this.url = url;
        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Connecting");
        // Set a flag indicating we have inherent keep-alive in this transport.
        connection.features.inherentKeepAlive = true;
        if (transferFormat === TransferFormat.Binary && (typeof new XMLHttpRequest().responseType !== "string")) {
            // This will work if we fix: https://github.com/aspnet/SignalR/issues/742
            throw new Error("Binary protocols over XmlHttpRequest not implementing advanced features are not supported.");
        }
        this.poll(this.url, transferFormat);
        return Promise.resolve();
    };
    LongPollingTransport.prototype.poll = function (url, transferFormat) {
        return __awaiter(this, void 0, void 0, function () {
            var pollOptions, token, pollUrl, response, e_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        pollOptions = {
                            abortSignal: this.pollAbort.signal,
                            headers: {},
                            timeout: 90000,
                        };
                        if (transferFormat === TransferFormat.Binary) {
                            pollOptions.responseType = "arraybuffer";
                        }
                        token = this.accessTokenFactory();
                        if (token) {
                            // tslint:disable-next-line:no-string-literal
                            pollOptions.headers["Authorization"] = "Bearer " + token;
                        }
                        _a.label = 1;
                    case 1:
                        if (!!this.pollAbort.signal.aborted) return [3 /*break*/, 6];
                        _a.label = 2;
                    case 2:
                        _a.trys.push([2, 4, , 5]);
                        pollUrl = url + "&_=" + Date.now();
                        this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) polling: " + pollUrl);
                        return [4 /*yield*/, this.httpClient.get(pollUrl, pollOptions)];
                    case 3:
                        response = _a.sent();
                        if (response.statusCode === 204) {
                            this.logger.log(ILogger_1.LogLevel.Information, "(LongPolling transport) Poll terminated by server");
                            // Poll terminated by server
                            if (this.onclose) {
                                this.onclose();
                            }
                            this.pollAbort.abort();
                        }
                        else if (response.statusCode !== 200) {
                            this.logger.log(ILogger_1.LogLevel.Error, "(LongPolling transport) Unexpected response code: " + response.statusCode);
                            // Unexpected status code
                            if (this.onclose) {
                                this.onclose(new Errors_1.HttpError(response.statusText, response.statusCode));
                            }
                            this.pollAbort.abort();
                        }
                        else {
                            // Process the response
                            if (response.content) {
                                this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) data received. " + getDataDetail(response.content) + ".");
                                if (this.onreceive) {
                                    this.onreceive(response.content);
                                }
                            }
                            else {
                                // This is another way timeout manifest.
                                this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                            }
                        }
                        return [3 /*break*/, 5];
                    case 4:
                        e_1 = _a.sent();
                        if (e_1 instanceof Errors_1.TimeoutError) {
                            // Ignore timeouts and reissue the poll.
                            this.logger.log(ILogger_1.LogLevel.Trace, "(LongPolling transport) Poll timed out, reissuing.");
                        }
                        else {
                            // Close the connection with the error as the result.
                            if (this.onclose) {
                                this.onclose(e_1);
                            }
                            this.pollAbort.abort();
                        }
                        return [3 /*break*/, 5];
                    case 5: return [3 /*break*/, 1];
                    case 6: return [2 /*return*/];
                }
            });
        });
    };
    LongPollingTransport.prototype.send = function (data) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                return [2 /*return*/, send(this.logger, "LongPolling", this.httpClient, this.url, this.accessTokenFactory, data)];
            });
        });
    };
    LongPollingTransport.prototype.stop = function () {
        this.pollAbort.abort();
        return Promise.resolve();
    };
    return LongPollingTransport;
}());
exports.LongPollingTransport = LongPollingTransport;
function getDataDetail(data) {
    var length = null;
    if (data instanceof ArrayBuffer) {
        length = "Binary data of length " + data.byteLength;
    }
    else if (typeof data === "string") {
        length = "String data of length " + data.length;
    }
    return length;
}
function send(logger, transportName, httpClient, url, accessTokenFactory, content) {
    return __awaiter(this, void 0, void 0, function () {
        var headers, token, response, _a;
        return __generator(this, function (_b) {
            switch (_b.label) {
                case 0:
                    token = accessTokenFactory();
                    if (token) {
                        headers = (_a = {},
                            _a["Authorization"] = "Bearer " + accessTokenFactory(),
                            _a);
                    }
                    logger.log(ILogger_1.LogLevel.Trace, "(" + transportName + " transport) sending data. " + getDataDetail(content) + ".");
                    return [4 /*yield*/, httpClient.post(url, {
                            content: content,
                            headers: headers,
                        })];
                case 1:
                    response = _b.sent();
                    logger.log(ILogger_1.LogLevel.Trace, "(" + transportName + " transport) request complete. Response status: " + response.statusCode + ".");
                    return [2 /*return*/];
            }
        });
    });
}
//# sourceMappingURL=Transports.js.map