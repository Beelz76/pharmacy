<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Справочники</h1>
    <router-view />
  </div>
</template>

<script setup>
import { onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";

const router = useRouter();
const route = useRoute();

onMounted(() => {
  if (route.path === "/admin/references") {
    router.replace({ name: "AdminCategories" });
  }
});
</script>
