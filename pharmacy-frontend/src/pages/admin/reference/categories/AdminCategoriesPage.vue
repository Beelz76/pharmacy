<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">Категории</h1>
    <div class="mb-4 text-gray-600">
      Всего категорий: {{ flatCategories.length }}
    </div>

    <div class="flex justify-between mb-4">
      <el-button type="primary" @click="openCreate">
        <i class="fas fa-plus mr-1"></i> Создать
      </el-button>
    </div>

    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead
          class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
        >
          <tr>
            <th class="px-6 py-5 font-semibold">Название</th>
            <th class="px-6 py-5 font-semibold">Описание</th>
            <th class="px-6 py-5 font-semibold">Действия</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="c in tableCategories" :key="c.id">
            <td class="px-6 py-4 whitespace-nowrap">
              <span :style="{ paddingLeft: `${c.indent * 1.5}rem` }">{{
                c.name
              }}</span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">{{ c.description }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              <div class="flex justify-start gap-2">
                <el-button size="small" @click.stop="editCategory(c)">
                  <i class="fas fa-edit" />
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="removeCategory(c)"
                >
                  <i class="fas fa-trash" />
                </el-button>
              </div>
            </td>
          </tr>
          <tr v-if="tableCategories.length === 0">
            <td colspan="3" class="text-center py-6 text-gray-500">
              Категории не найдены
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <el-dialog
      v-model="dialogVisible"
      title="Категория"
      width="700px"
      :close-on-click-modal="false"
    >
      <el-form
        label-width="120px"
        :model="form"
        :rules="rules"
        ref="formRef"
        class="pb-2"
      >
        <el-form-item label="Название" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="Описание" prop="description">
          <el-input v-model="form.description" />
        </el-form-item>
        <el-form-item label="Родитель">
          <el-select v-model="form.parentCategoryId" clearable>
            <el-option :value="null" label="Нет" />
            <el-option
              v-for="c in flatCategories"
              :key="c.id"
              :value="c.id"
              :label="c.name"
            />
          </el-select>
        </el-form-item>
      </el-form>

      <h3 class="font-semibold mb-2">Поля</h3>
      <el-table :data="fields" size="small" row-key="localId" class="mb-2">
        <el-table-column prop="label" label="Метка">
          <template #default="scope">
            <el-input v-model="scope.row.label" />
          </template>
        </el-table-column>
        <el-table-column prop="key" label="Ключ" width="150">
          <template #default="scope">
            <el-input v-model="scope.row.key" />
          </template>
        </el-table-column>
        <el-table-column prop="type" label="Тип" width="140">
          <template #default="scope">
            <el-select v-model="scope.row.type" placeholder="Тип">
              <el-option
                v-for="t in fieldTypes"
                :key="t"
                :label="t"
                :value="t"
              />
            </el-select>
          </template>
        </el-table-column>
        <el-table-column
          prop="isRequired"
          label="Обяз."
          width="80"
          align="center"
        >
          <template #default="scope">
            <el-checkbox v-model="scope.row.isRequired" />
          </template>
        </el-table-column>
        <el-table-column
          prop="isFilterable"
          label="Фильтр"
          width="80"
          align="center"
        >
          <template #default="scope">
            <el-checkbox v-model="scope.row.isFilterable" />
          </template>
        </el-table-column>
        <el-table-column width="70">
          <template #default="scope">
            <el-button
              size="small"
              type="danger"
              @click="deleteField(scope.$index)"
            >
              <i class="fas fa-trash" />
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-button size="small" @click="addField">Добавить поле</el-button>

      <template #footer>
        <el-button @click="dialogVisible = false">Отмена</el-button>
        <el-button type="primary" @click="save">Сохранить</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from "vue";
import {
  getAllCategories,
  getCategoryById,
  createCategory,
  updateCategory,
  deleteCategory,
  addCategoryFields,
  updateCategoryFields,
  deleteCategoryFields,
  getCategoryFields,
} from "/src/services/CategoryService";
import { ElMessage, ElMessageBox } from "element-plus";

const categories = ref([]);
const tableCategories = ref([]);
const flatCategories = ref([]);
const loaded = ref(false);
const dialogVisible = ref(false);

const formRef = ref();
const rules = {
  name: [{ required: true, message: "Введите название", trigger: "blur" }],
  description: [
    { required: true, message: "Введите описание", trigger: "blur" },
  ],
};

const form = reactive({
  id: null,
  name: "",
  description: "",
  parentCategoryId: null,
});
const fields = ref([]);
let removedFieldIds = [];
let localIdCounter = 1;
const fieldTypes = ["string", "number", "integer", "boolean", "date"];

function flatten(list, arr = []) {
  for (const c of list) {
    arr.push({ id: c.id, name: c.name });
    if (c.subcategories?.length) flatten(c.subcategories, arr);
  }
  return arr;
}

function flattenForTable(list, indent = 0, arr = []) {
  for (const c of list) {
    arr.push({ ...c, indent });
    if (c.subcategories?.length)
      flattenForTable(c.subcategories, indent + 1, arr);
  }
  return arr;
}

async function load() {
  loaded.value = false;
  try {
    categories.value = await getAllCategories();
    flatCategories.value = flatten(categories.value);
    tableCategories.value = flattenForTable(categories.value);
  } finally {
    loaded.value = true;
  }
}

function addField() {
  fields.value.push({
    localId: `new_${localIdCounter++}`,
    id: null,
    key: "",
    label: "",
    type: "string",
    isRequired: false,
    isFilterable: false,
  });
}

function deleteField(index) {
  const field = fields.value[index];
  if (field.id) {
    removedFieldIds.push(field.id);
  }
  fields.value.splice(index, 1);
}

async function openCreate() {
  form.id = null;
  form.name = "";
  form.description = "";
  form.parentCategoryId = null;
  fields.value = [];
  removedFieldIds = [];
  dialogVisible.value = true;
}

async function editCategory(row) {
  form.id = row.id;
  const cat = await getCategoryById(row.id);
  form.name = cat.name;
  form.description = cat.description;
  form.parentCategoryId = cat.parentCategoryId;
  const catFields = await getCategoryFields(row.id);
  fields.value = catFields.map((f) => ({
    localId: f.id,
    ...f,
  }));
  removedFieldIds = [];
  dialogVisible.value = true;
}

async function save() {
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;
  try {
    if (form.id) {
      await updateCategory(form.id, {
        name: form.name,
        description: form.description,
        parentCategoryId: form.parentCategoryId,
      });
      const newFields = fields.value.filter((f) => !f.id);
      const updatedFields = fields.value.filter((f) => f.id);
      if (newFields.length) {
        await addCategoryFields(form.id, newFields.map(mapField));
      }
      if (updatedFields.length) {
        await updateCategoryFields(form.id, updatedFields.map(mapField));
      }
      if (removedFieldIds.length) {
        await deleteCategoryFields(form.id, removedFieldIds);
      }
      ElMessage.success("Категория обновлена");
    } else {
      await createCategory({
        name: form.name,
        description: form.description,
        parentCategoryId: form.parentCategoryId,
        fields: fields.value.map(mapField),
      });
      ElMessage.success("Категория создана");
    }
    dialogVisible.value = false;
    await load();
  } catch {}
}

function mapField(f) {
  return {
    id: f.id,
    key: f.key,
    label: f.label,
    type: f.type,
    isRequired: f.isRequired,
    isFilterable: f.isFilterable,
  };
}

async function removeCategory(row) {
  try {
    await ElMessageBox.confirm("Удалить категорию?", "Подтверждение", {
      confirmButtonText: "Удалить",
      cancelButtonText: "Отмена",
      type: "warning",
    });
  } catch {
    return;
  }
  try {
    await deleteCategory(row.id);
    await load();
    ElMessage.success("Категория удалена");
  } catch {}
}

onMounted(load);
</script>
