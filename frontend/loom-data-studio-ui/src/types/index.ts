// Enums matching backend exactly
export type SchemaRole = 'Incoming' | 'Master' | 'Outgoing'
export type SchemaStatus = 'Draft' | 'Published' | 'Archived'
export type FieldType = 'Scalar' | 'Object' | 'Array'
export type ScalarType = 'String' | 'Integer' | 'Decimal' | 'Boolean' | 'Date' | 'DateTime' | 'Time' | 'Guid'
export type FlowType = 'IncomingToMaster' | 'MasterToOutgoing'
export type RuleType = 'Field' | 'CrossField' | 'Conditional'
export type Severity = 'Error' | 'Warning'
export type TransformationMode = 'Simple' | 'Advanced'
export type Cardinality = 'OneToOne' | 'OneToMany'
export type TransformNodeType = 'Source' | 'Map' | 'Filter' | 'Aggregate' | 'Join' | 'Split' | 'Constant' | 'Expression'

// Data Models
export interface DataModel {
  id: string
  tenantId: string
  key: string
  name: string
  description: string | null
  createdAt: string
}

// Data Schemas
export interface DataSchemaSummary {
  id: string
  tenantId: string
  dataModelId: string | null
  role: SchemaRole
  key: string
  version: number
  status: SchemaStatus
  description: string | null
  createdAt: string
  publishedAt: string | null
  tags?: string[]
}

export interface FieldDefinitionSummary {
  id: string
  path: string
  fieldType: FieldType
  scalarType: ScalarType | null
  elementSchemaId: string | null
  required: boolean
  description: string | null
}

export interface KeyFieldSummary {
  id: string
  fieldPath: string
  order: number
  normalization: string | null
}

export interface KeyDefinitionSummary {
  id: string
  name: string
  isPrimary: boolean
  keyFields: KeyFieldSummary[]
}

export interface DataSchemaDetails {
  id: string
  tenantId: string
  dataModelId: string | null
  role: SchemaRole
  key: string
  version: number
  status: SchemaStatus
  description: string | null
  createdAt: string
  publishedAt: string | null
  fields: FieldDefinitionSummary[]
  tags: string[]
  keyDefinitions: KeyDefinitionSummary[]
}

export interface SchemaGraphNode {
  schemaId: string
  key: string
  version: number
  role: SchemaRole
  status: SchemaStatus
}

export interface SchemaGraphEdge {
  fromSchemaId: string
  toSchemaId: string
  fieldPath: string
}

export interface SchemaGraph {
  rootSchemaId: string
  nodes: SchemaGraphNode[]
  edges: SchemaGraphEdge[]
}

// Validation
export interface ValidationRuleSummary {
  id: string
  ruleType: RuleType
  severity: Severity
  parameters: string // JSONB
}

export interface ValidationReferenceSummary {
  id: string
  fieldPath: string
  childValidationSpecId: string
}

export interface ValidationSpecDetails {
  id: string
  tenantId: string
  dataSchemaId: string
  version: number
  status: SchemaStatus
  description: string | null
  createdAt: string
  publishedAt: string | null
  rules: ValidationRuleSummary[]
  references: ValidationReferenceSummary[]
}

export interface ValidationError {
  field: string
  message: string
}

export interface ValidationResult {
  isValid: boolean
  errors: ValidationError[]
}

// Transformation
export interface SimpleTransformRuleSummary {
  id: string
  sourcePath: string
  targetPath: string
  converterId: string | null
  required: boolean
  order: number
}

export interface TransformGraphNodeSummary {
  id: string
  key: string
  nodeType: TransformNodeType
  outputType: string // JSON descriptor
  config: string // JSONB
}

export interface TransformGraphEdgeSummary {
  id: string
  fromNodeId: string
  toNodeId: string
  inputName: string
  order: number
}

export interface TransformOutputBindingSummary {
  id: string
  targetPath: string
  fromNodeId: string
}

export interface TransformReferenceSummary {
  id: string
  sourceFieldPath: string
  targetFieldPath: string
  childTransformationSpecId: string
}

export interface TransformationSpecDetails {
  id: string
  tenantId: string
  sourceSchemaId: string
  targetSchemaId: string
  mode: TransformationMode
  cardinality: Cardinality
  version: number
  status: SchemaStatus
  description: string | null
  createdAt: string
  publishedAt: string | null
  simpleRules: SimpleTransformRuleSummary[]
  graphNodes: TransformGraphNodeSummary[]
  graphEdges: TransformGraphEdgeSummary[]
  outputBindings: TransformOutputBindingSummary[]
  references: TransformReferenceSummary[]
}

export interface CompiledSimpleTransformRule {
  sourcePath: string
  targetPath: string
  converterId: string | null
  required: boolean
}

export interface CompiledTransformGraphNode {
  nodeType: TransformNodeType
  outputType: string
  config: string
}

export interface CompiledTransformGraphEdge {
  fromNodeKey: string
  toNodeKey: string
  inputName: string
}

export interface CompiledTransformOutputBinding {
  targetPath: string
  fromNodeKey: string
}

export interface CompiledTransformReference {
  sourceFieldPath: string
  targetFieldPath: string
  childTransformationSpecId: string
}

export interface CompiledTransformationSpec {
  id: string
  sourceSchemaId: string
  targetSchemaId: string
  mode: TransformationMode
  cardinality: Cardinality
  simpleRules: CompiledSimpleTransformRule[]
  graphNodes: Record<string, CompiledTransformGraphNode>
  graphEdges: CompiledTransformGraphEdge[]
  outputBindings: CompiledTransformOutputBinding[]
  references: CompiledTransformReference[]
}

// API Response Types
export interface IdResponse {
  id: string
}

export interface SuccessResponse {
  success: boolean
}

