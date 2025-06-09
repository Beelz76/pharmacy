<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">Пользователи</h1>
    <div class="mb-4 text-gray-600">Всего пользователей: {{ totalCount }}</div>

    <div class="bg-white rounded-lg shadow p-6 mb-6">
      <el-form :inline="true" @submit.prevent>
        <el-form-item label-width="0">
          <el-input
            v-model="filters.firstName"
            placeholder="Имя"
            size="large"
            class="!w-48"
          />
        </el-form-item>
        <el-form-item label-width="0">
          <el-input
            v-model="filters.lastName"
            placeholder="Фамилия"
            size="large"
            class="!w-48"
          />
        </el-form-item>
        <el-form-item label-width="0">
          <el-input
            v-model="filters.email"
            placeholder="Email"
            size="large"
            class="!w-52"
          />
        </el-form-item>
        <el-form-item label="Роль">
          <el-select
            v-model="filters.role"
            placeholder="Все"
            clearable
            class="!w-40"
          >
            <el-option label="Администратор" value="Admin" />
            <el-option label="Сотрудник" value="Employee" />
            <el-option label="Пользователь" value="User" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" plain @click="fetch">Поиск</el-button>
          <el-button @click="resetFilters">Сбросить</el-button>
        </el-form-item>
      </el-form>
    </div>

    <div class="flex justify-end mb-4">
      <el-button type="primary" @click="goCreate"
        ><i class="fas fa-plus mr-1"></i> Новый пользователь</el-button
      >
    </div>

    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead
          class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
        >
          <tr>
            <th class="px-6 py-5 font-semibold">ID</th>
            <th class="px-6 py-5 font-semibold">Email</th>
            <th class="px-6 py-5 font-semibold">Имя</th>
            <th class="px-6 py-5 font-semibold">Роль</th>
            <th class="px-6 py-5 font-semibold text-right">
              <span class="sr-only">Детали</span>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="u in users"
            :key="u.id"
            class="hover:bg-secondary-50 cursor-pointer"
            @click="goDetails(u.id)"
          >
            <td class="px-6 py-4 whitespace-nowrap">{{ u.id }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ u.email }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ fullName(u) }}</td>
            <td class="px-6 py-4 whitespace-nowrap capitalize">{{ u.role }}</td>
            <td class="px-6 py-4 text-right text-gray-400">
              <i class="fas fa-chevron-right"></i>
            </td>
          </tr>
          <tr v-if="!loading && users.length === 0">
            <td colspan="5" class="text-center py-6 text-gray-500">
              Пользователи не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="5" class="text-center py-6 text-gray-500">
              Загрузка...
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-if="totalCount > pageSize" class="flex justify-center mt-6">
      <el-pagination
        layout="sizes, prev, pager, next"
        :total="totalCount"
        :page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        v-model:page-size="pageSize"
        v-model:current-page="pageNumber"
      />
    </div>
  </div>
</template>

<script setup>
import { reactive, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useUsers } from "/src/services/UserService";

const route = useRoute();
const router = useRouter();
const filters = reactive({
  firstName: "",
  lastName: "",
  email: "",
  role: null,
});
const { users, totalCount, pageNumber, pageSize, loading, fetchUsers } =
  useUsers();

pageNumber.value = Number(route.query.page) || 1;
pageSize.value = Number(route.query.size) || pageSize.value;

const fetch = () => {
  fetchUsers({ page: pageNumber.value, size: pageSize.value, filters });
};

const resetFilters = () => {
  filters.firstName = "";
  filters.lastName = "";
  filters.email = "";
  filters.role = null;
  pageNumber.value = 1;
  fetch();
};

watch(pageNumber, (val) => {
  router.replace({
    query: { ...route.query, page: val, size: pageSize.value },
  });
  fetch();
});

watch(pageSize, (val) => {
  pageNumber.value = 1;
  router.replace({ query: { ...route.query, page: 1, size: val } });
  fetch();
});
const goDetails = (id) => {
  router.push({
    name: "AdminUserDetails",
    params: { id },
    query: { page: pageNumber.value },
  });
};

const goCreate = () => {
  router.push({ name: "AdminUserCreate" });
};

const fullName = (u) =>
  [u.lastName, u.firstName, u.patronymic].filter(Boolean).join(" ");

fetch();
</script>
