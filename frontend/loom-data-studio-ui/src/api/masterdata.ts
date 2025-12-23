import { api } from './client'
import type {
  DataSchemaSummary,
  DataSchemaDetails,
  SchemaGraph,
  ValidationSpecDetails,
  ValidationResult,
  TransformationSpecDetails,
  CompiledTransformationSpec,
  CompatibleTransformationSpecSummary,
  IdResponse,
  SuccessResponse,
  SchemaRole,
  SchemaStatus,
} from '../types'

const TENANT_ID = '00000000-0000-0000-0000-000000000001'

export const dataModelsApi = {
  createDataModel: (key: string, name: string, description?: string) =>
    api.post<IdResponse>('/datamodels', {
      tenantId: TENANT_ID,
      key,
      name,
      description,
    }),
}

export const schemasApi = {
  getSchemas: (role?: SchemaRole, status?: SchemaStatus) => {
    const params = new URLSearchParams({ tenantId: TENANT_ID })
    if (role) params.append('role', role)
    if (status) params.append('status', status)
    return api.get<DataSchemaSummary[]>(`/schemas?${params.toString()}`)
  },

  getSchemaDetails: (schemaId: string) =>
    api.get<DataSchemaDetails>(`/schemas/${schemaId}`),

  getSchemaGraph: (schemaId: string) =>
    api.get<SchemaGraph>(`/schemas/${schemaId}/graph`),

  createSchema: (
    dataModelId: string | null,
    role: SchemaRole,
    key: string,
    description?: string
  ) =>
    api.post<IdResponse>('/schemas', {
      tenantId: TENANT_ID,
      dataModelId,
      role,
      key,
      description,
    }),

  addField: (
    schemaId: string,
    path: string,
    fieldType: string,
    scalarType: string | null,
    elementSchemaId: string | null,
    required: boolean,
    description?: string
  ) =>
    api.post<IdResponse>(`/schemas/${schemaId}/fields`, {
      path,
      fieldType,
      scalarType,
      elementSchemaId,
      required,
      description,
    }),

  removeField: (fieldId: string) =>
    api.delete<SuccessResponse>(`/schemas/fields/${fieldId}`),

  updateField: (
    fieldId: string,
    path?: string,
    fieldType?: string,
    scalarType?: string | null,
    elementSchemaId?: string | null,
    required?: boolean,
    description?: string
  ) =>
    api.put<SuccessResponse>(`/schemas/fields/${fieldId}`, {
      path,
      fieldType,
      scalarType,
      elementSchemaId,
      required,
      description,
    }),

  publishSchema: (schemaId: string, publishedBy: string) =>
    api.post<SuccessResponse>(`/schemas/${schemaId}/publish`, {
      publishedBy,
    }),

  validateSchema: (schemaId: string) =>
    api.get<ValidationResult>(`/schemas/${schemaId}/validate`),

  getUnpublishedDependencies: (schemaId: string) =>
    api.get<UnpublishedDependency[]>(`/schemas/${schemaId}/unpublished-dependencies`),

  publishRelatedSchemas: (schemaId: string, publishedBy: string, relatedSchemaIds: string[]) =>
    api.post<PublishRelatedSchemasResponse>(`/schemas/${schemaId}/publish-related`, {
      publishedBy,
      relatedSchemaIds,
    }),

  deleteSchemaVersion: (schemaVersionId: string) =>
    api.delete<SuccessResponse>(`/schemas/${schemaVersionId}`),

  deleteSchema: (key: string, role: SchemaRole) => {
    const params = new URLSearchParams({ tenantId: TENANT_ID, key, role })
    return api.delete<SuccessResponse>(`/schemas?${params.toString()}`)
  },

  addSchemaTag: (schemaId: string, tag: string) =>
    api.post<IdResponse>(`/schemas/${schemaId}/tags`, { tag }),

  removeSchemaTag: (tagId: string) =>
    api.delete<SuccessResponse>(`/schemas/tags/${tagId}`),

  removeSchemaTagByValue: (schemaId: string, tag: string) => {
    const params = new URLSearchParams({ tag })
    return api.delete<SuccessResponse>(`/schemas/${schemaId}/tags?${params.toString()}`)
  },
}

export interface UnpublishedDependency {
  schemaId: string
  key: string
  version: number
  status: string
  role: string
}

export interface PublishRelatedSchemasResponse {
  publishedSchemaIds: string[]
}

export const validationSpecsApi = {
  getValidationSpecDetails: (specId: string) =>
    api.get<ValidationSpecDetails>(`/validationspecs/${specId}`),

  getValidationSpecBySchemaId: (schemaId: string) =>
    api.get<ValidationSpecDetails>(`/validationspecs/by-schema/${schemaId}`),

  validateValidationSpec: (specId: string) =>
    api.get<ValidationResult>(`/validationspecs/${specId}/validate`),

  createValidationSpec: (dataSchemaId: string, description?: string) =>
    api.post<IdResponse>('/validationspecs', {
      tenantId: TENANT_ID,
      dataSchemaId,
      description,
    }),

  addValidationRule: (
    specId: string,
    ruleType: string,
    severity: string,
    parameters: string
  ) =>
    api.post<IdResponse>(`/validationspecs/${specId}/rules`, {
      ruleType,
      severity,
      parameters,
    }),

  removeValidationRule: (ruleId: string) =>
    api.delete<SuccessResponse>(`/validationspecs/rules/${ruleId}`),

  updateValidationRule: (
    ruleId: string,
    ruleType?: string,
    severity?: string,
    parameters?: string
  ) =>
    api.put<SuccessResponse>(`/validationspecs/rules/${ruleId}`, {
      ruleType,
      severity,
      parameters,
    }),

  addValidationReference: (
    specId: string,
    fieldPath: string,
    childValidationSpecId: string
  ) =>
    api.post<IdResponse>(`/validationspecs/${specId}/references`, {
      fieldPath,
      childValidationSpecId,
    }),

  publishValidationSpec: (specId: string, publishedBy: string) =>
    api.post<SuccessResponse>(`/validationspecs/${specId}/publish`, {
      publishedBy,
    }),
}

export const transformationSpecsApi = {
  getTransformationSpecDetails: (specId: string) =>
    api.get<TransformationSpecDetails>(`/transformationspecs/${specId}`),

  getTransformationSpecBySourceSchemaId: (sourceSchemaId: string) =>
    api.get<TransformationSpecDetails>(`/transformationspecs/by-source-schema/${sourceSchemaId}`),

  getCompiledTransformationSpec: (specId: string) =>
    api.get<CompiledTransformationSpec>(`/transformationspecs/${specId}/compiled`),

  validateTransformationSpec: (specId: string) =>
    api.get<ValidationResult>(`/transformationspecs/${specId}/validate`),

  createTransformationSpec: (
    sourceSchemaId: string,
    targetSchemaId: string,
    mode: string,
    cardinality: string,
    description?: string
  ) =>
    api.post<IdResponse>('/transformationspecs', {
      tenantId: TENANT_ID,
      sourceSchemaId,
      targetSchemaId,
      mode,
      cardinality,
      description,
    }),

  addSimpleTransformRule: (
    specId: string,
    sourcePath: string,
    targetPath: string,
    converterId: string | null,
    required: boolean,
    order: number
  ) =>
    api.post<IdResponse>(`/transformationspecs/${specId}/simple-rules`, {
      sourcePath,
      targetPath,
      converterId,
      required,
      order,
    }),

  removeSimpleTransformRule: (ruleId: string) =>
    api.delete<SuccessResponse>(`/transformationspecs/simple-rules/${ruleId}`),

  getCompatibleTransformationSpecs: (
    sourceSchemaId: string,
    targetSchemaId: string,
    status?: SchemaStatus
  ) => {
    const params = new URLSearchParams({
      sourceSchemaId,
      targetSchemaId,
    })
    if (status) params.append('status', status)
    return api.get<CompatibleTransformationSpecSummary[]>(
      `/transformationspecs/compatible?${params.toString()}`
    )
  },

  updateSimpleTransformRule: (
    ruleId: string,
    sourcePath?: string,
    targetPath?: string,
    converterId?: string | null,
    required?: boolean
  ) =>
    api.put<SuccessResponse>(`/transformationspecs/simple-rules/${ruleId}`, {
      sourcePath,
      targetPath,
      converterId,
      required,
    }),

  addTransformReference: (
    specId: string,
    sourceFieldPath: string,
    targetFieldPath: string,
    childTransformationSpecId: string
  ) =>
    api.post<IdResponse>(`/transformationspecs/${specId}/references`, {
      sourceFieldPath,
      targetFieldPath,
      childTransformationSpecId,
    }),

  publishTransformationSpec: (specId: string, publishedBy: string) =>
    api.post<SuccessResponse>(`/transformationspecs/${specId}/publish`, {
      publishedBy,
    }),

  addTransformGraphNode: (
    specId: string,
    key: string,
    nodeType: string,
    outputType: string,
    config: string
  ) =>
    api.post<IdResponse>(`/transformationspecs/${specId}/graph-nodes`, {
      key,
      nodeType,
      outputType,
      config,
    }),

  removeTransformGraphNode: (nodeId: string) =>
    api.delete<SuccessResponse>(`/transformationspecs/graph-nodes/${nodeId}`),

  addTransformGraphEdge: (
    specId: string,
    fromNodeId: string,
    toNodeId: string,
    inputName: string,
    order: number
  ) =>
    api.post<IdResponse>(`/transformationspecs/${specId}/graph-edges`, {
      fromNodeId,
      toNodeId,
      inputName,
      order,
    }),

  removeTransformGraphEdge: (edgeId: string) =>
    api.delete<SuccessResponse>(`/transformationspecs/graph-edges/${edgeId}`),

  addTransformOutputBinding: (
    specId: string,
    targetPath: string,
    fromNodeId: string
  ) =>
    api.post<IdResponse>(`/transformationspecs/${specId}/output-bindings`, {
      targetPath,
      fromNodeId,
    }),
}

export const keyDefinitionsApi = {
  addKeyDefinition: (
    dataSchemaId: string,
    name: string,
    isPrimary: boolean
  ) =>
    api.post<IdResponse>('/keydefinitions', {
      tenantId: TENANT_ID,
      dataSchemaId,
      name,
      isPrimary,
    }),

  addKeyField: (
    keyDefinitionId: string,
    fieldPath: string,
    order: number,
    normalization?: string
  ) =>
    api.post<IdResponse>(`/keydefinitions/${keyDefinitionId}/fields`, {
      fieldPath,
      order,
      normalization,
    }),

  removeKeyField: (fieldId: string) =>
    api.delete<SuccessResponse>(`/keydefinitions/fields/${fieldId}`),

  removeKeyDefinition: (keyDefinitionId: string) =>
    api.delete<SuccessResponse>(`/keydefinitions/${keyDefinitionId}`),

  reorderKeyFields: (keyDefinitionId: string, keyFieldIdsInOrder: string[]) =>
    api.put<SuccessResponse>(`/keydefinitions/${keyDefinitionId}/fields/reorder`, {
      keyFieldIdsInOrder,
    }),

  getSchemaBusinessKeys: (schemaId: string) =>
    api.get<BusinessKeysResponse>(`/keydefinitions/schemas/${schemaId}`),
}

interface BusinessKeysResponse {
  keys: BusinessKeyDto[]
}

interface BusinessKeyDto {
  id: string
  name: string
  isPrimary: boolean
  createdAt: string
  fields: BusinessKeyFieldDto[]
}

interface BusinessKeyFieldDto {
  id: string
  fieldPath: string
  order: number
  normalization: string | null
}

