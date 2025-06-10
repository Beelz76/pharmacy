import api from "../utils/axios";

export async function getPharmacies({ page = 1, size = 10, search = "" } = {}) {
  const params = new URLSearchParams({
    pageNumber: page,
    pageSize: size,
  });
  if (search) params.append("search", search);
  const res = await api.get(`/pharmacies?${params.toString()}`);
  return res.data;
}

export async function getPharmacyById(id) {
  const res = await api.get(`/pharmacies/${id}`);
  return res.data;
}

export async function createPharmacy(payload) {
  const res = await api.post("/pharmacies", payload);
  return res.data;
}

export async function updatePharmacy(id, payload) {
  await api.put(`/pharmacies/${id}`, payload);
}

export async function deletePharmacy(id) {
  await api.delete(`/pharmacies/${id}`);
}

export async function getPharmacyProducts(pharmacyId) {
  const res = await api.get(`/pharmacy/${pharmacyId}/products`);
  return res.data;
}

export async function addPharmacyProduct(pharmacyId, payload) {
  const res = await api.post(`/pharmacy/${pharmacyId}/products`, payload);
  return res.data;
}

export async function updatePharmacyProduct(pharmacyId, productId, payload) {
  await api.put(`/pharmacy/${pharmacyId}/products/${productId}`, payload);
}

export async function deletePharmacyProduct(pharmacyId, productId) {
  await api.delete(`/pharmacy/${pharmacyId}/products/${productId}`);
}
