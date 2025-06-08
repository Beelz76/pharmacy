<template>
  <div>
    <HeaderUser v-if="showHeader" />
    <router-view />
    <LoginRegisterModal v-model:visible="showLoginModal" />
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, computed } from "vue";
import { useRoute } from "vue-router";
import HeaderUser from "../src/components/HeaderUser.vue";
import LoginRegisterModal from "./components/LoginRegisterModal.vue";
import { useAuthStore } from "./stores/AuthStore";

const auth = useAuthStore();
const showLoginModal = ref(false);
const route = useRoute();
const showHeader = computed(() => route.meta.layout !== "admin");

function openLoginModal() {
  showLoginModal.value = true;
}

onMounted(() => {
  auth.initialize();
  window.addEventListener("unauthorized", openLoginModal);
});

onBeforeUnmount(() => {
  window.removeEventListener("unauthorized", openLoginModal);
});
</script>

<style>
body.el-popup-parent--hidden {
  padding-right: 0 !important;
}
</style>
