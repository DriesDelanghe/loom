import type { NodeType, NodeCategory } from '../types'

export const nodeCategoryMap: Record<NodeType, NodeCategory> = {
  Action: 'Action',
  Condition: 'Condition',
  Validation: 'Validation',
  Split: 'Control',
  Join: 'Control',
}

export const isControlNode = (nodeType: NodeType): boolean => {
  return nodeCategoryMap[nodeType] === 'Control'
}

export const getAllowedOutcomes = (nodeType: NodeType): string[] => {
  switch (nodeType) {
    case 'Action':
      return ['Completed', 'Failed']
    case 'Condition':
      return ['True', 'False']
    case 'Validation':
      return ['Valid', 'Invalid']
    case 'Split':
      return ['Next']
    case 'Join':
      return ['Joined']
    default:
      return []
  }
}

export const getPositiveOutcome = (nodeType: NodeType): string => {
  switch (nodeType) {
    case 'Action':
      return 'Completed'
    case 'Condition':
      return 'True'
    case 'Validation':
      return 'Valid'
    case 'Split':
      return 'Next'
    case 'Join':
      return 'Joined'
    default:
      return 'Completed'
  }
}

export const getNegativeOutcome = (nodeType: NodeType): string => {
  switch (nodeType) {
    case 'Action':
      return 'Failed'
    case 'Condition':
      return 'False'
    case 'Validation':
      return 'Invalid'
    default:
      return 'Failed'
  }
}

export const getOutcomeFromHandleId = (handleId: string, nodeType: NodeType): string => {
  // Handle IDs: "source-Completed", "source-Failed", etc. for non-control nodes
  // For control nodes, all use "Next" or "Joined"
  if (isControlNode(nodeType)) {
    return nodeType === 'Split' ? 'Next' : 'Joined'
  }

  // Extract outcome from handle ID (e.g., "source-Completed" -> "Completed")
  const parts = handleId.split('-')
  if (parts.length >= 2) {
    return parts.slice(1).join('-')
  }
  
  // Fallback: determine by position (top = positive, bottom = negative)
  if (handleId.includes('top') || handleId.includes('positive')) {
    return getPositiveOutcome(nodeType)
  }
  return getNegativeOutcome(nodeType)
}

