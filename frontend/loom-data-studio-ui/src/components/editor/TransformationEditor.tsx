import { AdvancedTransformationEditor } from './AdvancedTransformationEditor'
import { SimpleTransformationEditor } from './SimpleTransformationEditor'
import type { SchemaRole, TransformationMode } from '../../types'

interface TransformationEditorProps {
  schemaId: string
  role: SchemaRole
  isReadOnly: boolean
  expertMode: boolean
  onRuleSelect: (ruleId: string | null) => void
  selectedRuleId: string | null
  mode: TransformationMode
}

export function TransformationEditor({
  schemaId,
  role,
  isReadOnly,
  expertMode: _expertMode,
  onRuleSelect,
  selectedRuleId,
  mode,
}: TransformationEditorProps) {
  return (
    <div className="h-full flex flex-col">
      {mode === 'Simple' ? (
        <SimpleTransformationEditor
          schemaId={schemaId}
          role={role}
          isReadOnly={isReadOnly}
          onRuleSelect={onRuleSelect}
          selectedRuleId={selectedRuleId}
        />
      ) : (
        <AdvancedTransformationEditorWrapper
          schemaId={schemaId}
          role={role}
          isReadOnly={isReadOnly}
        />
      )}
    </div>
  )
}


function AdvancedTransformationEditorWrapper({
  schemaId,
  role: _role,
  isReadOnly,
}: {
  schemaId: string
  role: SchemaRole
  isReadOnly: boolean
}) {
  // In a real implementation, we'd find or create a transformation spec
  // For now, we'll use a placeholder ID
  const transformationSpecId = schemaId // This should be the actual transformation spec ID

  return (
    <div className="h-full">
      <AdvancedTransformationEditor
        transformationSpecId={transformationSpecId}
        isReadOnly={isReadOnly}
      />
    </div>
  )
}

