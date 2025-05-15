import api from '../utils/axios'

export async function getOrderById(orderId) {
  try {
    const response = await api.get(`/orders/${orderId}`)
    return response.data
  } catch (err) {
    console.error(`Ошибка при получении заказа #${orderId}:`, err)
    throw err
  }
}

export async function cancelOrder(orderId) {
  try {
    await api.put(`/orders/${orderId}/cancel`)
  } catch (err) {
    console.error(`Ошибка при отмене заказа #${orderId}:`, err)
    throw err
  }
}

export async function payOrder(orderId) {
  try {
    await api.post(`/orders/${orderId}/pay`)
  } catch (err) {
    console.error(`Ошибка при оплате заказа #${orderId}:`, err)
    throw err
  }
}

export async function refundOrder(orderId) {
  try {
    await api.put(`/orders/${orderId}/refund`)
  } catch (err) {
    console.error(`Ошибка при возврате средств по заказу #${orderId}:`, err)
    throw err
  }
}

export async function updateOrderStatus(orderId, newStatus) {
  try {
    await api.put(`/orders/${orderId}/status`, { newStatus })
  } catch (err) {
    console.error(`Ошибка при обновлении статуса заказа #${orderId}:`, err)
    throw err
  }
}

export async function getOrderStatuses() {
  try {
    const response = await api.get('/orders/statuses')
    return response.data
  } catch (err) {
    console.error('Ошибка при получении статусов заказов:', err)
    throw err
  }
}

export function useOrders() {
  const orders = ref([])
  const totalCount = ref(0)
  const pageNumber = ref(1)
  const pageSize = ref(20)
  const loading = ref(false)
  const error = ref(null)

  const fetchOrders = async ({
    page = 1,
    size = 20,
    sortBy = null,
    sortOrder = null,
    filters = {}
  } = {}) => {
    loading.value = true
    error.value = null

    try {
      const data = await getPaginatedOrders({ page, size, sortBy, sortOrder, filters })
      orders.value = data.items
      totalCount.value = data.totalCount
      pageNumber.value = data.pageNumber
      pageSize.value = data.pageSize
    } catch (err) {
      error.value = err
      orders.value = []
      totalCount.value = 0
    } finally {
      loading.value = false
    }
  }

  return {
    orders,
    totalCount,
    pageNumber,
    pageSize,
    loading,
    error,
    fetchOrders
  }
}
