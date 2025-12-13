import axios from "axios";
import { Order, Holding, Transaction, Wallet, Stock, StockQuote } from "../types";

const API_BASE_URL = "https://localhost:7212";
const API_URL = `${API_BASE_URL}/api`;

const http = axios.create({ baseURL: API_URL });

http.interceptors.request.use((config) => {
  const token = localStorage.getItem("accessToken");
  if (token) {
    config.headers = config.headers ?? {};
    (config.headers as any).Authorization = `Bearer ${token}`;
  }
  return config;
});

// reads user id from jwt
function getUserIdFromToken(): number | null {
  const token = localStorage.getItem("accessToken");
  if (!token) return null;
  try {
    const [, payloadB64] = token.split(".");
    const json = JSON.parse(atob(payloadB64));
    const sub = json.sub;
    const id = typeof sub === "string" ? Number(sub) : Number(sub);
    return Number.isFinite(id) ? id : null;
  } catch {
    return null;
  }
}

// api functions

export async function getOrders(): Promise<Order[]> {
  const res = await http.get("/order");
  return res.data;
}

export async function getHoldings(): Promise<Holding[]> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.get(`/holdings/user/${userId}`);
  return res.data;
}

export async function getTransactions(): Promise<Transaction[]> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.get(`/order/user/${userId}/transactions`);
  return res.data;
}

export async function getStocks(): Promise<Stock[]> {
  const res = await http.get("/Stock");
  return res.data;
}

export async function getWalletBalance(): Promise<Wallet> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.get(`/Wallet/user/${userId}/balance`);
  return res.data;
}

export async function getWallet(): Promise<Wallet> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.get(`/Wallet/${userId}`);
  return res.data as Wallet;
}

export async function getLiveQuote(symbol: string): Promise<StockQuote> {
  const res = await http.get(`/Stock/live/${symbol}`);
  return res.data;
}

export async function createOrder(dto: {
  stockId: number;
  quantity: number;
  type: "BUY" | "SELL";
}): Promise<Order> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.post("/order", { ...dto, userId });
  return res.data;
}

export async function login(username: string, password: string): Promise<void> {
  const res = await axios.post(`${API_BASE_URL}/api/auth/login`, { username, password });
  const { accessToken } = res.data as { accessToken: string };
  localStorage.setItem("accessToken", accessToken);
}
