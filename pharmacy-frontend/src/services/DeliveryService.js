import api from "../utils/axios";

export async function getDeliveries({
  page = 1,
  size = 20,
  filters = {},
} = {}) {
  const params = new URLSearchParams({ pageNumber: page, pageSize: size });
  const res = await api.post(
    `/deliveries/paginated?${params.toString()}`,
    filters
  );
  return res.data;
}

export async function getDeliveryByOrderId(orderId) {
  const res = await api.get(`/deliveries/order/${orderId}`);
  return res.data;
}
