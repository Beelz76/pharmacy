import api from "../utils/axios";

export default {
  async getById(id) {
    const res = await api.get(`/products/${id}`);
    return res.data;
  },

  async create(productData) {
    const res = await api.post("/products", productData);
    return res.data;
  },

  async update(id, productData) {
    const res = await api.put(`/products/${id}`, productData);
    return res.data;
  },

  async delete(id) {
    await api.delete(`/products/${id}`);
  },

  async getBySku(sku) {
    const res = await api.get(`/products/by-sku/${sku}`);
    return res.data;
  },

  async getFilterValues(categoryId) {
    const res = await api.get(`/products/filter-values/${categoryId}`);
    return res.data;
  },
};
