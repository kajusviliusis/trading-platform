import axios from "axios";
import { Order, Holding, Transaction, Wallet, Stock, StockQuote } from "../types";

const API_BASE_URL = "http://localhost:5001";
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

// JWT helpers
function base64UrlDecode(input: string): string {
  const b64 = input.replace(/-/g, "+").replace(/_/g, "/");
  const pad = b64.length % 4 === 2 ? "==" : b64.length % 4 === 3 ? "=" : "";
  return atob(b64 + pad);
}

type JwtPayload = {
  sub?: string | number;
  exp?: number;
  [key: string]: unknown;
};

function getJwtPayload(): JwtPayload | null {
  const token = localStorage.getItem("accessToken");
  if (!token) return null;
  try {
    const parts = token.split(".");
    if (parts.length !== 3) return null;
    const payloadJson = base64UrlDecode(parts[1]);
    const payload = JSON.parse(payloadJson) as JwtPayload;
    return payload;
  } catch {
    return null;
  }
}

function isTokenExpired(payload: JwtPayload | null): boolean {
  if (!payload?.exp) return true;
  const nowSeconds = Math.floor(Date.now() / 1000);
  return payload.exp <= nowSeconds;
}

export function isAuthenticated(): boolean {
  const payload = getJwtPayload();
  return !!payload && !isTokenExpired(payload);
}

export function logout(): void {
  localStorage.removeItem("accessToken");
}

function getUserIdFromToken(): number | null {
  const payload = getJwtPayload();
  if (!payload || isTokenExpired(payload)) return null;
  const sub = payload.sub;
  const id = typeof sub === "string" ? Number(sub) : typeof sub === "number" ? sub : NaN;
  return Number.isFinite(id) ? id : null;
}

export function getCurrentUserId(): number | null {
  return getUserIdFromToken();
}

// API functions

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

export async function getLiveQuote(symbol: string): Promise<StockQuote> {
  const res = await http.get(`/Stock/live/${symbol}`);
  return res.data;
}

export async function getWalletBalance(): Promise<Wallet> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.get(`/Wallet/user/${userId}/balance`);
  return res.data;
}

export async function getUserWallet(): Promise<{ id: number; balance: number; currency: string; userId: number }> {
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  const res = await http.get("/Wallet");
  const wallets = res.data as Array<{ id: number; balance: number; currency: string; userId: number }>;
  const wallet = wallets.find((w) => w.userId === userId);
  if (!wallet) throw new Error("Wallet not found for user");
  return wallet;
}

export async function createWallet(dto: { balance: number; currency: string; userId: number }) {
  const res = await http.post("/Wallet", dto);
  return res.data as { id: number; balance: number; currency: string; userId: number };
}

export async function updateWallet(walletId: number, dto: { balance: number; currency: string }): Promise<Wallet> {
  const res = await http.put(`/Wallet/${walletId}`, dto);
  return res.data as Wallet;
}

export async function depositFunds(amount: number): Promise<Wallet> {
  if (!Number.isFinite(amount) || amount <= 0) throw new Error("Amount must be greater than 0");
  const userId = getUserIdFromToken();
  if (userId == null) throw new Error("Not authenticated");
  let wallet: { id: number; balance: number; currency: string; userId: number };
  try {
    wallet = await getUserWallet();
  } catch (err: any) {
    const msg = err?.message ?? "";
    if (msg.includes("Wallet not found for user")) {
      const created = await createWallet({ balance: 0, currency: "USD", userId });
      wallet = created;
    } else {
      throw err;
    }
  }
  return await updateWallet(wallet.id, {
    balance: wallet.balance + amount,
    currency: wallet.currency,
  });
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

export async function register(username: string, password: string): Promise<void> {
  await axios.post(`${API_BASE_URL}/api/auth/register`, { username, password });
}

export async function getUserById(id: number): Promise<{ id: number; username: string; email: string; createdAt: string }> {
  const res = await http.get(`/User/${id}`);
  return res.data;
}
