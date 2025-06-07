import api from "../utils/axios";

export async function getUserAddresses() {
  const res = await api.get("/users/addresses");
  return res.data;
}

export async function createUserAddress(payload) {
  const res = await api.post("/users/addresses", payload);
  return res.data;
}

export async function updateUserAddress(id, payload) {
  await api.post(`/users/addresses/${id}`, payload);
}

export async function deleteUserAddress(id) {
  await api.delete(`/users/addresses/${id}`);
}
