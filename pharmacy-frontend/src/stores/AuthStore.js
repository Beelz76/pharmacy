import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { useAccountStore } from "./AccountStore";
import { useCartStore } from "./CartStore";
import { useFavoritesStore } from "./FavoritesStore";

export const useAuthStore = defineStore("auth", () => {
  const token = ref(localStorage.getItem("token"));
  const email = ref(null);
  const role = ref(null);
  const userId = ref(null);
  const returnUrl = ref(null);

  const isAuthenticated = computed(() => !!token.value);

  function setToken(newToken) {
    token.value = newToken;
    localStorage.setItem("token", newToken);

    try {
      const payload = JSON.parse(atob(newToken.split(".")[1]));
      email.value = payload.email || null;
      role.value =
        payload[
          "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        ] || null;
      userId.value = payload.sub;
    } catch (e) {
      email.value = null;
      role.value = null;
      userId.value = null;
    }

    useCartStore().syncToServer();
    useFavoritesStore().syncToServer();
  }

  function initialize() {
    const savedToken = localStorage.getItem("token");
    if (savedToken) {
      setToken(savedToken);
    }
    const savedReturnUrl = localStorage.getItem("returnUrl");
    if (savedReturnUrl) {
      returnUrl.value = savedReturnUrl;
    }
  }

  function setEmail(newEmail) {
    email.value = newEmail;
  }

  function setReturnUrl(url) {
    returnUrl.value = url;
    localStorage.setItem("returnUrl", url);
  }

  function clearReturnUrl() {
    returnUrl.value = null;
    localStorage.removeItem("returnUrl");
  }

  function logout() {
    token.value = null;
    email.value = null;
    role.value = null;
    userId.value = null;
    returnUrl.value = null;
    localStorage.removeItem("token");

    useAccountStore().clear();
    useCartStore().$reset();
    useFavoritesStore().$reset();

    window.location.href = "/";
  }

  function softLogout() {
    token.value = null;
    email.value = null;
    role.value = null;
    userId.value = null;
    returnUrl.value = null;
    localStorage.removeItem("token");

    useAccountStore().clear();
    useCartStore().$reset();
    useFavoritesStore().$reset();
  }

  if (token.value) {
    setToken(token.value);
  }

  return {
    token,
    email,
    role,
    userId,
    returnUrl,
    isAuthenticated,
    setToken,
    setReturnUrl,
    clearReturnUrl,
    logout,
    initialize,
  };
});
