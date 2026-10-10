<template>
  <UDashboardPanel id="home">
    <template #header>
      <UDashboardNavbar title="Inicio">
        <template #leading>
          <UDashboardSidebarCollapse />
        </template>
      </UDashboardNavbar>
    </template>

    <template #body>
      <UPageCard
        title="Proyecto base"
        description="La autenticación, los usuarios, roles, permisos y la auditoría están listos para reutilizar."
      />
    </template>
  </UDashboardPanel>
</template>
