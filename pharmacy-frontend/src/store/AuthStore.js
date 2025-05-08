import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('token'))
  const email = ref(null)
  const role = ref(null)
  const userId = ref(null)

  const isAuthenticated = computed(() => !!token.value)

  function setToken(newToken) {
    token.value = newToken
    localStorage.setItem('token', newToken)

    try {
      const payload = JSON.parse(atob(newToken.split('.')[1]))
      email.value = payload.email || null
      role.value = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null
      userId.value = payload.sub
      window.location.reload()
    } catch (e) {
      email.value = null
      role.value = null
      userId.value = null
    }
  }

  function logout() {
    token.value = null
    email.value = null
    role.value = null
    userId.value = null
    localStorage.removeItem('token')
    window.location.href = '/'
  }

  return {
    token,
    email,
    role,
    userId,
    isAuthenticated,
    setToken,
    logout
  }
})
