<template>
  <div class="flex flex-col md:flex-row min-h-screen bg-gray-50">
    <AdminSidebar />
    <main class="flex-1 p-4 md:p-8">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import AdminSidebar from "../components/sidebars/AdminSidebar.vue";
</script>
