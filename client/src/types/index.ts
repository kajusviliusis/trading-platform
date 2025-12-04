export interface Order {
	id: number;
	quantity: number;
	type: "BUY" | "SELL";
	priceAtExecution: number;
	timestamp: string;
	userId: number;
	stockId: number;
}

export interface Holding {
	stockId: number;
	quantity: number;
	stockName: string;
	currentPrice: number;
}

export interface Transaction {
	id: number;
	userId: number;
	stockId: number;
	orderId: number;
	quantity: number;
	priceAtExecution: number;
	timestamp: string;
	type: "BUY" | "SELL";
}
export interface Wallet {
	id: number;
	balance: number;
	currency: string;
	userId: number;
}
export interface Stock {
	id: number;
	symbol: string;
	name: string;
	price: number;
}
