import api from "../utils/axios";

export async function getPayments({ page = 1, size = 20, filters = {} } = {}) {
  const params = new URLSearchParams({ pageNumber: page, pageSize: size });
  const res = await api.post(
    `/payments/paginated?${params.toString()}`,
    filters
  );
  return res.data;
}

export async function getPaymentById(id) {
  const res = await api.get(`/payments/${id}`);
  return res.data;
}

export async function updatePaymentStatus(orderId, newStatus) {
  await api.put("/payments/status", { orderId, newStatus });
}

export async function syncPaymentStatus(id) {
  const res = await api.post(`/payments/${id}/sync`);
  return res.data;
}
