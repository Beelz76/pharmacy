import api from "../utils/axios";

export async function getPaymentMethods() {
  const res = await api.get("/payment-methods");
  return res.data;
}

export async function getPaymentStatuses() {
  const res = await api.get("/payments/statuses");
  return res.data;
}
