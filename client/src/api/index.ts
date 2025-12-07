import axios from "axios";
import { Order, Holding, Transaction, Wallet, Stock, StockQuote } from "../types";

const API_URL = "https://localhost:7212/api";
const USER_ID = 1; //hardcoded paskui pakeist 

export async function getOrders(): Promise<Order[]> {
    const res = await axios.get(`${API_URL}/order`);
    return res.data;
}

export async function getHoldings(): Promise<Holding[]> {
    const res = await axios.get(`${API_URL}/holdings/user/${USER_ID}`);
    return res.data;
}

export async function getTransactions(): Promise<Transaction[]> {
    const res = await axios.get(`${API_URL}/order/user/${USER_ID}/transactions`);
    return res.data;
}
export async function getStocks(): Promise<Stock[]> {
    const res = await axios.get(`${API_URL}/Stock`);
    return res.data;
}
export async function getWalletBalance(): Promise<Wallet> {
    const res = await axios.get(`${API_URL}/Wallet/user/${USER_ID}/balance`);
    return res.data;
}
export async function getWallet(): Promise<Wallet> {
  const res = await axios.get(`${ API_URL }/Wallet/${ USER_ID }`);
  return res.data as Wallet;
}
export async function getLiveQuote(symbol: string): Promise<StockQuote> {
    const res = await axios.get(`${API_URL}/Stock/live/${symbol}`);
    return res.data;
}
export async function createOrder(dto: {
    stockId: number;
    quantity: number;
    type: "BUY" | "SELL";
}): Promise<Order> {
    const res = await axios.post(`${API_URL}/order`, { ...dto, userId: USER_ID });
    return res.data;
}
