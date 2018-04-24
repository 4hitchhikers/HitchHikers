import { DataReceived, TransportClosed } from "./Common";
import { HttpClient } from "./HttpClient";
import { IConnection } from "./IConnection";
import { ILogger } from "./ILogger";
export declare enum TransportType {
    WebSockets = 0,
    ServerSentEvents = 1,
    LongPolling = 2,
}
export declare enum TransferFormat {
    Text = 1,
    Binary = 2,
}
export interface ITransport {
    connect(url: string, transferFormat: TransferFormat, connection: IConnection): Promise<void>;
    send(data: any): Promise<void>;
    stop(): Promise<void>;
    onreceive: DataReceived;
    onclose: TransportClosed;
}
export declare class WebSocketTransport implements ITransport {
    private readonly logger;
    private readonly accessTokenFactory;
    private webSocket;
    constructor(accessTokenFactory: () => string, logger: ILogger);
    connect(url: string, transferFormat: TransferFormat, connection: IConnection): Promise<void>;
    send(data: any): Promise<void>;
    stop(): Promise<void>;
    onreceive: DataReceived;
    onclose: TransportClosed;
}
export declare class ServerSentEventsTransport implements ITransport {
    private readonly httpClient;
    private readonly accessTokenFactory;
    private readonly logger;
    private eventSource;
    private url;
    constructor(httpClient: HttpClient, accessTokenFactory: () => string, logger: ILogger);
    connect(url: string, transferFormat: TransferFormat, connection: IConnection): Promise<void>;
    send(data: any): Promise<void>;
    stop(): Promise<void>;
    onreceive: DataReceived;
    onclose: TransportClosed;
}
export declare class LongPollingTransport implements ITransport {
    private readonly httpClient;
    private readonly accessTokenFactory;
    private readonly logger;
    private url;
    private pollXhr;
    private pollAbort;
    constructor(httpClient: HttpClient, accessTokenFactory: () => string, logger: ILogger);
    connect(url: string, transferFormat: TransferFormat, connection: IConnection): Promise<void>;
    private poll(url, transferFormat);
    send(data: any): Promise<void>;
    stop(): Promise<void>;
    onreceive: DataReceived;
    onclose: TransportClosed;
}
