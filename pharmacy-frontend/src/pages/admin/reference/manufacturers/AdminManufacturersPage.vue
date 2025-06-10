<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">Производители</h1>
    <div class="mb-4 text-gray-600">
      Всего производителей: {{ list.length }}
    </div>

    <div class="flex justify-between mb-4">
      <el-button type="primary" @click="openCreate">
        <i class="fas fa-plus mr-1"></i> Добавить
      </el-button>
    </div>

    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead
          class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
        >
          <tr>
            <th class="px-6 py-5 font-semibold">ID</th>
            <th class="px-6 py-5 font-semibold">Название</th>
            <th class="px-6 py-5 font-semibold">Страна</th>
            <th class="px-6 py-5 font-semibold">Действия</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="m in list" :key="m.id">
            <td class="px-6 py-4">{{ m.id }}</td>
            <td class="px-6 py-4">{{ m.name }}</td>
            <td class="px-6 py-4">{{ m.country }}</td>
            <td class="px-6 py-4">
              <div class="flex justify-left gap-2">
                <el-button size="small" @click="edit(m)">
                  <i class="fas fa-edit" />
                </el-button>
                <el-button size="small" type="danger" @click="remove(m.id)">
                  <i class="fas fa-trash" />
                </el-button>
              </div>
            </td>
          </tr>
          <tr v-if="list.length === 0">
            <td colspan="4" class="text-center py-6 text-gray-500">
              Нет данных
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <el-dialog v-model="dialogVisible" title="Производитель" width="500px">
      <el-form label-width="120px">
        <el-form-item label="Название">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="Страна">
          <el-input v-model="form.country" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">Отмена</el-button>
        <el-button type="primary" @click="save">Сохранить</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import { ElMessage, ElMessageBox } from "element-plus";
import {
  getManufacturers,
  createManufacturer,
  updateManufacturer,
  deleteManufacturer,
} from "/src/services/ManufacturerService";

const list = ref([]);
const dialogVisible = ref(false);
const form = reactive({ id: null, name: "", country: "" });

async function load() {
  list.value = await getManufacturers();
}

function openCreate() {
  form.id = null;
  form.name = "";
  form.country = "";
  dialogVisible.value = true;
}

function edit(row) {
  form.id = row.id;
  form.name = row.name;
  form.country = row.country;
  dialogVisible.value = true;
}

async function save() {
  try {
    if (form.id) {
      await updateManufacturer(form.id, {
        name: form.name,
        country: form.country,
      });
    } else {
      await createManufacturer({ name: form.name, country: form.country });
    }
    ElMessage.success("Сохранено");
    dialogVisible.value = false;
    await load();
  } catch (e) {
    ElMessage.error(
      `${e.response?.status} ${e.response?.data?.message || e.message}`
    );
  }
}

async function remove(id) {
  try {
    await ElMessageBox.confirm("Удалить производителя?", "Подтверждение", {
      confirmButtonText: "Удалить",
      cancelButtonText: "Отмена",
      type: "warning",
    });
  } catch {
    return;
  }
  try {
    await deleteManufacturer(id);
    ElMessage.success("Удалено");
    await load();
  } catch (e) {
    ElMessage.error(
      `${e.response?.status} ${e.response?.data?.message || e.message}`
    );
  }
}

onMounted(load);
</script>
