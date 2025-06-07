import api from "../utils/axios";
import { ref } from "vue";

export async function getUserById(userId) {
  const res = await api.get(`/users/${userId}`);
  return res.data;
}

export async function createUser(payload) {
  const res = await api.post("/users", payload);
  return res.data;
}

export async function getPaginatedUsers({
  page = 1,
  size = 20,
  filters = {},
} = {}) {
  const query = new URLSearchParams({
    pageNumber: page,
    pageSize: size,
  });
  const res = await api.post(`/users/paginated?${query.toString()}`, filters);
  return res.data;
}

export function useUsers() {
  const users = ref([]);
  const totalCount = ref(0);
  const pageNumber = ref(1);
  const pageSize = ref(20);
  const loading = ref(false);
  const error = ref(null);

  const fetchUsers = async ({ page = 1, size = 20, filters = {} } = {}) => {
    loading.value = true;
    error.value = null;
    try {
      const data = await getPaginatedUsers({ page, size, filters });
      users.value = data.items;
      totalCount.value = data.totalCount;
      pageNumber.value = data.pageNumber;
      pageSize.value = data.pageSize;
    } catch (err) {
      error.value = err;
      users.value = [];
      totalCount.value = 0;
    } finally {
      loading.value = false;
    }
  };

  return {
    users,
    totalCount,
    pageNumber,
    pageSize,
    loading,
    error,
    fetchUsers,
  };
}
