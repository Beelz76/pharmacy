import api from "../utils/axios";

export async function getManufacturers() {
  const res = await api.get("/manufacturers");
  return res.data;
}

export async function createManufacturer(payload) {
  const res = await api.post("/manufacturers", payload);
  return res.data;
}

export async function updateManufacturer(id, payload) {
  await api.put(`/manufacturers/${id}`, payload);
}

export async function deleteManufacturer(id) {
  await api.delete(`/manufacturers/${id}`);
}
