import api from "../utils/axios";

export async function getOrderById(orderId) {
  try {
    const response = await api.get(`/orders/${orderId}`);
    return response.data;
  } catch (err) {
    console.error(`Ошибка при получении заказа #${orderId}:`, err);
    throw err;
  }
}

export async function cancelOrder(orderId) {
  try {
    await api.put(`/orders/${orderId}/cancel`);
  } catch (err) {
    console.error(`Ошибка при отмене заказа #${orderId}:`, err);
    throw err;
  }
}

export async function payOrder(orderId) {
  try {
    const res = await api.post(`/orders/${orderId}/pay`);
    return res.data;
  } catch (err) {
    console.error(`Ошибка при оплате заказа #${orderId}:`, err);
    throw err;
  }
}

export async function refundOrder(orderId) {
  try {
    await api.put(`/orders/${orderId}/refund`);
  } catch (err) {
    console.error(`Ошибка при возврате средств по заказу #${orderId}:`, err);
    throw err;
  }
}

export async function updateOrderStatus(orderId, newStatus) {
  try {
    await api.put(`/orders/${orderId}/status`, { newStatus });
  } catch (err) {
    console.error(`Ошибка при обновлении статуса заказа #${orderId}:`, err);
    throw err;
  }
}

export async function getOrderStatuses() {
  try {
    const response = await api.get("/orders/statuses");
    return response.data;
  } catch (err) {
    console.error("Ошибка при получении статусов заказов:", err);
    throw err;
  }
}
