// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
import { LogLevel } from "./ILogger";
import { NullLogger } from "./Loggers";
import { TextMessageFormat } from "./TextMessageFormat";
import { TransferFormat } from "./Transports";
export const JSON_HUB_PROTOCOL_NAME = "json";
export class JsonHubProtocol {
    constructor() {
        this.name = JSON_HUB_PROTOCOL_NAME;
        this.version = 1;
        this.transferFormat = TransferFormat.Text;
    }
    parseMessages(input, logger) {
        if (!input) {
            return [];
        }
        if (logger === null) {
            logger = new NullLogger();
        }
        // Parse the messages
        const messages = TextMessageFormat.parse(input);
        const hubMessages = [];
        for (const message of messages) {
            const parsedMessage = JSON.parse(message);
            if (typeof parsedMessage.type !== "number") {
                throw new Error("Invalid payload.");
            }
            switch (parsedMessage.type) {
                case 1 /* Invocation */:
                    this.isInvocationMessage(parsedMessage);
                    break;
                case 2 /* StreamItem */:
                    this.isStreamItemMessage(parsedMessage);
                    break;
                case 3 /* Completion */:
                    this.isCompletionMessage(parsedMessage);
                    break;
                case 6 /* Ping */:
                    // Single value, no need to validate
                    break;
                case 7 /* Close */:
                    // All optional values, no need to validate
                    break;
                default:
                    // Future protocol changes can add message types, old clients can ignore them
                    logger.log(LogLevel.Information, "Unknown message type '" + parsedMessage.type + "' ignored.");
                    continue;
            }
            hubMessages.push(parsedMessage);
        }
        return hubMessages;
    }
    writeMessage(message) {
        return TextMessageFormat.write(JSON.stringify(message));
    }
    isInvocationMessage(message) {
        this.assertNotEmptyString(message.target, "Invalid payload for Invocation message.");
        if (message.invocationId !== undefined) {
            this.assertNotEmptyString(message.invocationId, "Invalid payload for Invocation message.");
        }
    }
    isStreamItemMessage(message) {
        this.assertNotEmptyString(message.invocationId, "Invalid payload for StreamItem message.");
        if (message.item === undefined) {
            throw new Error("Invalid payload for StreamItem message.");
        }
    }
    isCompletionMessage(message) {
        if (message.result && message.error) {
            throw new Error("Invalid payload for Completion message.");
        }
        if (!message.result && message.error) {
            this.assertNotEmptyString(message.error, "Invalid payload for Completion message.");
        }
        this.assertNotEmptyString(message.invocationId, "Invalid payload for Completion message.");
    }
    assertNotEmptyString(value, errorMessage) {
        if (typeof value !== "string" || value === "") {
            throw new Error(errorMessage);
        }
    }
}
//# sourceMappingURL=JsonHubProtocol.js.map