{{- define "loom-masterdata-configuration.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "loom-masterdata-configuration.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{- define "loom-masterdata-configuration.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "loom-masterdata-configuration.labels" -}}
helm.sh/chart: {{ include "loom-masterdata-configuration.chart" . }}
{{ include "loom-masterdata-configuration.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "loom-masterdata-configuration.selectorLabels" -}}
app.kubernetes.io/name: {{ include "loom-masterdata-configuration.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "loom-masterdata-configuration.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "loom-masterdata-configuration.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{- define "loom-masterdata-configuration.dbHost" -}}
{{- .Values.database.host }}
{{- end }}

{{- define "loom-masterdata-configuration.adminSecretName" -}}
{{- if .Values.database.existingAdminSecret }}
{{- .Values.database.existingAdminSecret }}
{{- else }}
{{- include "loom-masterdata-configuration.fullname" . }}-db-admin
{{- end }}
{{- end }}

{{- define "loom-masterdata-configuration.adminSecretKey" -}}
{{- default "password" .Values.database.adminSecretKey }}
{{- end }}

{{- define "loom-masterdata-configuration.serviceSecretName" -}}
{{- if .Values.database.existingServiceSecret }}
{{- .Values.database.existingServiceSecret }}
{{- else }}
{{- include "loom-masterdata-configuration.fullname" . }}-db-service
{{- end }}
{{- end }}

{{- define "loom-masterdata-configuration.serviceSecretPasswordKey" -}}
{{- default "password" .Values.database.serviceSecretPasswordKey }}
{{- end }}

{{- define "loom-masterdata-configuration.serviceSecretUsernameKey" -}}
{{- default "username" .Values.database.serviceSecretUsernameKey }}
{{- end }}

{{- define "loom-masterdata-configuration.serviceSecretConnectionStringKey" -}}
{{- default "connection-string" .Values.database.serviceSecretConnectionStringKey }}
{{- end }}

