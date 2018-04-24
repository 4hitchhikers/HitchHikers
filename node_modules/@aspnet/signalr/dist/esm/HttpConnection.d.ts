import { ConnectionClosed, DataReceived } from "./Common";
import { HttpClient } from "./HttpClient";
import { IConnection } from "./IConnection";
import { ILogger, LogLevel } from "./ILogger";
import { ITransport, TransferFormat, TransportType } from "./Transports";
export interface IHttpConnectionOptions {
    httpClient?: HttpClient;
    transport?: TransportType | ITransport;
    logger?: ILogger | LogLevel;
    accessTokenFactory?: () => string;
}
export declare class HttpConnection implements IConnection {
    private connectionState;
    private baseUrl;
    private url;
    private readonly httpClient;
    private readonly logger;
    private readonly options;
    private transport;
    private connectionId;
    private startPromise;
    readonly features: any;
    constructor(url: string, options?: IHttpConnectionOptions);
    start(transferFormat: TransferFormat): Promise<void>;
    private startInternal(transferFormat);
    private getNegotiationResponse(headers);
    private updateConnectionId(negotiateResponse);
    private createTransport(requestedTransport, negotiateResponse, requestedTransferFormat, headers);
    private constructTransport(transport);
    private resolveTransport(endpoint, requestedTransport, requestedTransferFormat);
    private isITransport(transport);
    private changeState(from, to);
    send(data: any): Promise<void>;
    stop(error?: Error): Promise<void>;
    private stopConnection(raiseClosed, error?);
    private resolveUrl(url);
    private resolveNegotiateUrl(url);
    onreceive: DataReceived;
    onclose: ConnectionClosed;
}
