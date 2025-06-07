import api from "../utils/axios";

export async function getPharmacies() {
  const res = await api.get("/pharmacies");
  return res.data;
}
