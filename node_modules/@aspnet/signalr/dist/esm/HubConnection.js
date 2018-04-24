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
import { HttpConnection } from "./HttpConnection";
import { LogLevel } from "./ILogger";
import { JsonHubProtocol } from "./JsonHubProtocol";
import { LoggerFactory } from "./Loggers";
import { Subject } from "./Observable";
import { TextMessageFormat } from "./TextMessageFormat";
export { JsonHubProtocol };
const DEFAULT_TIMEOUT_IN_MS = 30 * 1000;
export class HubConnection {
    constructor(urlOrConnection, options = {}) {
        options = options || {};
        this.timeoutInMilliseconds = options.timeoutInMilliseconds || DEFAULT_TIMEOUT_IN_MS;
        this.protocol = options.protocol || new JsonHubProtocol();
        if (typeof urlOrConnection === "string") {
            this.connection = new HttpConnection(urlOrConnection, options);
        }
        else {
            this.connection = urlOrConnection;
        }
        this.logger = LoggerFactory.createLogger(options.logger);
        this.connection.onreceive = (data) => this.processIncomingData(data);
        this.connection.onclose = (error) => this.connectionClosed(error);
        this.callbacks = {};
        this.methods = {};
        this.closedCallbacks = [];
        this.id = 0;
    }
    processIncomingData(data) {
        this.cleanupTimeout();
        if (!this.receivedHandshakeResponse) {
            data = this.processHandshakeResponse(data);
            this.receivedHandshakeResponse = true;
        }
        // Data may have all been read when processing handshake response
        if (data) {
            // Parse the messages
            const messages = this.protocol.parseMessages(data, this.logger);
            for (const message of messages) {
                switch (message.type) {
                    case 1 /* Invocation */:
                        this.invokeClientMethod(message);
                        break;
                    case 2 /* StreamItem */:
                    case 3 /* Completion */:
                        const callback = this.callbacks[message.invocationId];
                        if (callback != null) {
                            if (message.type === 3 /* Completion */) {
                                delete this.callbacks[message.invocationId];
                            }
                            callback(message);
                        }
                        break;
                    case 6 /* Ping */:
                        // Don't care about pings
                        break;
                    case 7 /* Close */:
                        this.logger.log(LogLevel.Information, "Close message received from server.");
                        this.connection.stop(message.error ? new Error("Server returned an error on close: " + message.error) : null);
                        break;
                    default:
                        this.logger.log(LogLevel.Warning, "Invalid message type: " + message.type);
                        break;
                }
            }
        }
        this.configureTimeout();
    }
    processHandshakeResponse(data) {
        let responseMessage;
        let messageData;
        let remainingData;
        try {
            if (data instanceof ArrayBuffer) {
                // Format is binary but still need to read JSON text from handshake response
                const binaryData = new Uint8Array(data);
                const separatorIndex = binaryData.indexOf(TextMessageFormat.RecordSeparatorCode);
                if (separatorIndex === -1) {
                    throw new Error("Message is incomplete.");
                }
                // content before separator is handshake response
                // optional content after is additional messages
                const responseLength = separatorIndex + 1;
                messageData = String.fromCharCode.apply(null, binaryData.slice(0, responseLength));
                remainingData = (binaryData.byteLength > responseLength) ? binaryData.slice(responseLength).buffer : null;
            }
            else {
                const textData = data;
                const separatorIndex = textData.indexOf(TextMessageFormat.RecordSeparator);
                if (separatorIndex === -1) {
                    throw new Error("Message is incomplete.");
                }
                // content before separator is handshake response
                // optional content after is additional messages
                const responseLength = separatorIndex + 1;
                messageData = textData.substring(0, responseLength);
                remainingData = (textData.length > responseLength) ? textData.substring(responseLength) : null;
            }
            // At this point we should have just the single handshake message
            const messages = TextMessageFormat.parse(messageData);
            responseMessage = JSON.parse(messages[0]);
        }
        catch (e) {
            const message = "Error parsing handshake response: " + e;
            this.logger.log(LogLevel.Error, message);
            const error = new Error(message);
            this.connection.stop(error);
            throw error;
        }
        if (responseMessage.error) {
            const message = "Server returned handshake error: " + responseMessage.error;
            this.logger.log(LogLevel.Error, message);
            this.connection.stop(new Error(message));
        }
        else {
            this.logger.log(LogLevel.Trace, "Server handshake complete.");
        }
        // multiple messages could have arrived with handshake
        // return additional data to be parsed as usual, or null if all parsed
        return remainingData;
    }
    configureTimeout() {
        if (!this.connection.features || !this.connection.features.inherentKeepAlive) {
            // Set the timeout timer
            this.timeoutHandle = setTimeout(() => this.serverTimeout(), this.timeoutInMilliseconds);
        }
    }
    serverTimeout() {
        // The server hasn't talked to us in a while. It doesn't like us anymore ... :(
        // Terminate the connection
        this.connection.stop(new Error("Server timeout elapsed without receiving a message from the server."));
    }
    invokeClientMethod(invocationMessage) {
        const methods = this.methods[invocationMessage.target.toLowerCase()];
        if (methods) {
            methods.forEach((m) => m.apply(this, invocationMessage.arguments));
            if (invocationMessage.invocationId) {
                // This is not supported in v1. So we return an error to avoid blocking the server waiting for the response.
                const message = "Server requested a response, which is not supported in this version of the client.";
                this.logger.log(LogLevel.Error, message);
                this.connection.stop(new Error(message));
            }
        }
        else {
            this.logger.log(LogLevel.Warning, `No client method with the name '${invocationMessage.target}' found.`);
        }
    }
    connectionClosed(error) {
        const callbacks = this.callbacks;
        this.callbacks = {};
        Object.keys(callbacks)
            .forEach((key) => {
            const callback = callbacks[key];
            callback(undefined, error ? error : new Error("Invocation canceled due to connection being closed."));
        });
        this.cleanupTimeout();
        this.closedCallbacks.forEach((c) => c.apply(this, [error]));
    }
    start() {
        return __awaiter(this, void 0, void 0, function* () {
            this.logger.log(LogLevel.Trace, "Starting HubConnection.");
            this.receivedHandshakeResponse = false;
            yield this.connection.start(this.protocol.transferFormat);
            this.logger.log(LogLevel.Trace, "Sending handshake request.");
            // Handshake request is always JSON
            yield this.connection.send(TextMessageFormat.write(JSON.stringify({ protocol: this.protocol.name, version: this.protocol.version })));
            this.logger.log(LogLevel.Information, `Using HubProtocol '${this.protocol.name}'.`);
            // defensively cleanup timeout in case we receive a message from the server before we finish start
            this.cleanupTimeout();
            this.configureTimeout();
        });
    }
    stop() {
        this.logger.log(LogLevel.Trace, "Stopping HubConnection.");
        this.cleanupTimeout();
        return this.connection.stop();
    }
    stream(methodName, ...args) {
        const invocationDescriptor = this.createStreamInvocation(methodName, args);
        const subject = new Subject(() => {
            const cancelInvocation = this.createCancelInvocation(invocationDescriptor.invocationId);
            const cancelMessage = this.protocol.writeMessage(cancelInvocation);
            delete this.callbacks[invocationDescriptor.invocationId];
            return this.connection.send(cancelMessage);
        });
        this.callbacks[invocationDescriptor.invocationId] = (invocationEvent, error) => {
            if (error) {
                subject.error(error);
                return;
            }
            if (invocationEvent.type === 3 /* Completion */) {
                if (invocationEvent.error) {
                    subject.error(new Error(invocationEvent.error));
                }
                else {
                    subject.complete();
                }
            }
            else {
                subject.next((invocationEvent.item));
            }
        };
        const message = this.protocol.writeMessage(invocationDescriptor);
        this.connection.send(message)
            .catch((e) => {
            subject.error(e);
            delete this.callbacks[invocationDescriptor.invocationId];
        });
        return subject;
    }
    send(methodName, ...args) {
        const invocationDescriptor = this.createInvocation(methodName, args, true);
        const message = this.protocol.writeMessage(invocationDescriptor);
        return this.connection.send(message);
    }
    invoke(methodName, ...args) {
        const invocationDescriptor = this.createInvocation(methodName, args, false);
        const p = new Promise((resolve, reject) => {
            this.callbacks[invocationDescriptor.invocationId] = (invocationEvent, error) => {
                if (error) {
                    reject(error);
                    return;
                }
                if (invocationEvent.type === 3 /* Completion */) {
                    const completionMessage = invocationEvent;
                    if (completionMessage.error) {
                        reject(new Error(completionMessage.error));
                    }
                    else {
                        resolve(completionMessage.result);
                    }
                }
                else {
                    reject(new Error(`Unexpected message type: ${invocationEvent.type}`));
                }
            };
            const message = this.protocol.writeMessage(invocationDescriptor);
            this.connection.send(message)
                .catch((e) => {
                reject(e);
                delete this.callbacks[invocationDescriptor.invocationId];
            });
        });
        return p;
    }
    on(methodName, newMethod) {
        if (!methodName || !newMethod) {
            return;
        }
        methodName = methodName.toLowerCase();
        if (!this.methods[methodName]) {
            this.methods[methodName] = [];
        }
        // Preventing adding the same handler multiple times.
        if (this.methods[methodName].indexOf(newMethod) !== -1) {
            return;
        }
        this.methods[methodName].push(newMethod);
    }
    off(methodName, method) {
        if (!methodName) {
            return;
        }
        methodName = methodName.toLowerCase();
        const handlers = this.methods[methodName];
        if (!handlers) {
            return;
        }
        if (method) {
            const removeIdx = handlers.indexOf(method);
            if (removeIdx !== -1) {
                handlers.splice(removeIdx, 1);
                if (handlers.length === 0) {
                    delete this.methods[methodName];
                }
            }
        }
        else {
            delete this.methods[methodName];
        }
    }
    onclose(callback) {
        if (callback) {
            this.closedCallbacks.push(callback);
        }
    }
    cleanupTimeout() {
        if (this.timeoutHandle) {
            clearTimeout(this.timeoutHandle);
        }
    }
    createInvocation(methodName, args, nonblocking) {
        if (nonblocking) {
            return {
                arguments: args,
                target: methodName,
                type: 1 /* Invocation */,
            };
        }
        else {
            const id = this.id;
            this.id++;
            return {
                arguments: args,
                invocationId: id.toString(),
                target: methodName,
                type: 1 /* Invocation */,
            };
        }
    }
    createStreamInvocation(methodName, args) {
        const id = this.id;
        this.id++;
        return {
            arguments: args,
            invocationId: id.toString(),
            target: methodName,
            type: 4 /* StreamInvocation */,
        };
    }
    createCancelInvocation(id) {
        return {
            invocationId: id,
            type: 5 /* CancelInvocation */,
        };
    }
}
//# sourceMappingURL=HubConnection.js.map