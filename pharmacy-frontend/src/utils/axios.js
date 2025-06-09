import axios from "axios";
import { ElMessage } from "element-plus";
import { useAuthStore } from "../stores/AuthStore";

const api = axios.create({
  baseURL: "https://localhost:7107",
  timeout: 10000,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status;
    const data = error.response?.data;
    let message = "Произошла ошибка запроса";

    if (data) {
      if (data.errors && typeof data.errors === "object") {
        message = Object.values(data.errors).flat().join(" ");
      } else if (Array.isArray(data.details) && data.details.length) {
        message = data.details.join(" ");
      } else if (data.message) {
        message = data.message;
      }
    }

    switch (status) {
      case 401: {
        try {
          useAuthStore().softLogout();
        } catch {
          localStorage.removeItem("token");
        }

        ElMessage.error("Сессия истекла. Пожалуйста, войдите заново.");
        window.dispatchEvent(new Event("unauthorized"));
        break;
      }

      case 403:
        ElMessage.warning("У вас нет прав для выполнения этого действия.");
        break;

      // case 404:
      //   ElMessage.warning("Ресурс не найден.");
      //   break;

      case 500:
        ElMessage.error("Внутренняя ошибка сервера. Попробуйте позже.");
        break;

      default:
        ElMessage.error(message);
        break;
    }

    return Promise.reject(error);
  }
);

export default api;
