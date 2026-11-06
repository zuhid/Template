<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { stringTypeStore } from '../store';

const tableRef = ref(null);
const router = useRouter();

const columns = [
  { key: 'charType', label: 'Char' },
  { key: 'varcharType', label: 'Varchar' },
  { key: 'textType', label: 'Text' },
  { key: 'actions', label: 'Actions', sortable: false },
];

onMounted(() => tableRef.value?.reload());

async function refresh() {
  await stringTypeStore.forceRefresh();
  await tableRef.value?.reload();
}

async function remove(row) {
  if (!confirm(`Delete "${row.varcharType}"?`)) return;
  const result = await stringTypeStore.delete(row.id);
  if (result !== undefined) await refresh();
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card id="string_types" title="String Types" saveText="" cancelText="">
    <Table ref="tableRef" :store="stringTypeStore" :columns="columns" filterable>
      <template #toolbar>
        <Button id="add_string_type" label="Add" @click="router.push('/string-types/new')" />
      </template>
      <template #actions="{ row }">
        <div class="actions">
          <Button :id="'edit_' + row.id" label="Edit" @click="router.push(`/string-types/${row.id}/edit`)" />
          <Button :id="'delete_' + row.id" label="Delete" @click="remove(row)" />
        </div>
      </template>
    </Table>
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#string_types {
  min-width: 700px;
}

.actions {
  display: flex;
  gap: var(--gap-md);
}
</style>
