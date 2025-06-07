<template>
  <div class="flex min-h-screen">
    <AdminSidebar />
    <main class="flex-1 p-6 bg-gray-50">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import AdminSidebar from '../components/sidebars/AdminSidebar.vue'
</script>