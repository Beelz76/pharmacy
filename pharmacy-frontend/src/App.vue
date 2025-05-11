<template>
  <div>
    <HeaderUser />
    <router-view />
    <LoginRegisterModal v-model:visible="showLoginModal" />
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import HeaderUser from '../src/components/HeaderUser.vue';
import LoginRegisterModal from './components/LoginRegisterModal.vue'
import { useAuthStore } from './store/AuthStore'

const auth = useAuthStore()
const showLoginModal = ref(false)

function openLoginModal() {
  showLoginModal.value = true
}

onMounted(() => {
  auth.initialize()
  window.addEventListener('unauthorized', openLoginModal)
})

onBeforeUnmount(() => {
  window.removeEventListener('unauthorized', openLoginModal)
})
</script>

<style>
body.el-popup-parent--hidden {
  padding-right: 0 !important;
}
</style>