import { ConnectionClosed, DataReceived } from "./Common";
import { TransferFormat } from "./Transports";
export interface IConnection {
    readonly features: any;
    start(transferFormat: TransferFormat): Promise<void>;
    send(data: any): Promise<void>;
    stop(error?: Error): Promise<void>;
    onreceive: DataReceived;
    onclose: ConnectionClosed;
}
