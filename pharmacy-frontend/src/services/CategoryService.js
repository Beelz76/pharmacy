import api from "../utils/axios";

export async function getAllCategories() {
  const res = await api.get("/categories");
  return res.data;
}

export async function getCategoryById(id) {
  const res = await api.get(`/categories/${id}`);
  return res.data;
}

export async function createCategoryApi(payload) {
  const res = await api.post("/categories", payload);
  return res.data;
}

export async function updateCategory(id, payload) {
  await api.put(`/categories/${id}/info`, payload);
}

export async function deleteCategory(id) {
  await api.delete(`/categories/${id}`);
}

export async function addCategoryFields(id, fields) {
  await api.post(`/categories/${id}/fields`, { fields });
}

export async function updateCategoryFields(id, fields) {
  await api.put(`/categories/${id}/fields`, { fields });
}

export async function deleteCategoryFields(id, fieldIds) {
  await api.delete(`/categories/${id}/fields`, { data: { fieldIds } });
}
