<template>
  <div class="flex flex-col md:flex-row max-w-7xl mx-auto px-0 py-6 gap-6">
    <AccountSidebar />
    <main class="flex-1 bg-white p-10 pt-4 rounded shadow">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import AccountSidebar from '../components/account/AccountSidebar.vue'
</script>
