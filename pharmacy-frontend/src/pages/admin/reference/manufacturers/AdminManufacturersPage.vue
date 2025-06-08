<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-xl font-semibold">Производители</h2>
      <el-button size="small" type="primary" @click="openCreate"
        >Добавить</el-button
      >
    </div>
    <el-table :data="list" style="width: 100%">
      <el-table-column prop="id" label="ID" width="80" />
      <el-table-column prop="name" label="Название" />
      <el-table-column prop="country" label="Страна" />
      <el-table-column width="120">
        <template #default="scope">
          <el-button size="small" @click="edit(scope.row)"
            >Редактировать</el-button
          >
          <el-button size="small" type="danger" @click="remove(scope.row.id)"
            >Удалить</el-button
          >
        </template>
      </el-table-column>
    </el-table>

    <el-dialog v-model="dialogVisible" title="Производитель">
      <el-form label-width="120px">
        <el-form-item label="Название">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="Страна">
          <el-input v-model="form.country" />
        </el-form-item>
      </el-form>
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="dialogVisible = false">Отмена</el-button>
          <el-button type="primary" @click="save">Сохранить</el-button>
        </span>
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
    ElMessage.error("Ошибка");
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
    ElMessage.error("Ошибка удаления");
  }
}

onMounted(load);
</script>
