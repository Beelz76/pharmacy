<template>
  <div>
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
