import api from '../utils/axios'

export default {
  async getById(id) {
    const res = await api.get(`/products/${id}`)
    return res.data
  },

  async create(productData) {
    const res = await api.post('/products', productData)
    return res.data
  },

  async update(id, productData) {
    const res = await api.put(`/products/${id}`, productData)
    return res.data
  },

  async delete(id) {
    await api.delete(`/products/${id}`)
  }
}
