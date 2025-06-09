import { ref } from "vue";
import api from "../utils/axios";

export async function getPaginatedOrders({
  page = 1,
  size = 10,
  sortBy = null,
  sortOrder = null,
  filters = {},
} = {}) {
  try {
    const query = new URLSearchParams({
      pageNumber: page,
      pageSize: size,
      ...(sortBy ? { sortBy } : {}),
      ...(sortOrder ? { sortOrder } : {}),
    });

    const response = await api.post(
      `/orders/paginated?${query.toString()}`,
      filters
    );
    return response.data;
  } catch (err) {
    console.error("Ошибка при получении заказов:", err);
    throw err;
  }
}

export function useOrders() {
  const orders = ref([]);
  const totalCount = ref(0);
  const pageNumber = ref(1);
  const pageSize = ref(20);
  const loading = ref(false);
  const error = ref(null);

  const fetchOrders = async ({
    page = 1,
    size = 20,
    sortBy = null,
    sortOrder = null,
    filters = {},
  } = {}) => {
    loading.value = true;
    error.value = null;

    try {
      const data = await getPaginatedOrders({
        page,
        size,
        sortBy,
        sortOrder,
        filters,
      });
      orders.value = data.items;
      totalCount.value = data.totalCount;
      pageNumber.value = data.pageNumber;
      pageSize.value = data.pageSize;
    } catch (err) {
      error.value = err;
      orders.value = [];
      totalCount.value = 0;
    } finally {
      loading.value = false;
    }
  };

  return {
    orders,
    totalCount,
    pageNumber,
    pageSize,
    loading,
    error,
    fetchOrders,
  };
}
