import axios from 'axios'
import { ElMessage } from 'element-plus'

const api = axios.create({
  baseURL: 'http://localhost:5068',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}, error => Promise.reject(error))

api.interceptors.response.use(
  response => response,
  error => {
    const status = error.response?.status
    const message = error.response?.data?.message || 'Произошла ошибка запроса'

    if (status === 401) {
      localStorage.removeItem('token')
      ElMessage.error('Сессия истекла. Пожалуйста, войдите заново.')
      setTimeout(() => {
        window.location.reload()
      }, 1000)
    } else {
      ElMessage.error(message)
    }

    return Promise.reject(error)
  }
)

export default api
