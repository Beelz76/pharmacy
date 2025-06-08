<template>
  <div class="flex min-h-screen bg-gray-50">
    <AdminSidebar />
    <main class="flex-1 p-8">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import AdminSidebar from '../components/sidebars/AdminSidebar.vue'
</script>