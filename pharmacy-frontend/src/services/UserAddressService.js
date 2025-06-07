import api from "../utils/axios";

export async function getUserAddresses() {
  const res = await api.get("/users/addresses");
  return res.data;
}

export async function createUserAddress(payload) {
  const res = await api.post("/users/addresses", payload);
  return res.data;
}
