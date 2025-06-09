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
