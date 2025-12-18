{{- define "loom-configuration.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "loom-configuration.fullname" -}}
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

{{- define "loom-configuration.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "loom-configuration.labels" -}}
helm.sh/chart: {{ include "loom-configuration.chart" . }}
{{ include "loom-configuration.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "loom-configuration.selectorLabels" -}}
app.kubernetes.io/name: {{ include "loom-configuration.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "loom-configuration.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "loom-configuration.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{- define "loom-configuration.dbHost" -}}
{{- .Values.database.host }}
{{- end }}

{{- define "loom-configuration.adminSecretName" -}}
{{- if .Values.database.existingAdminSecret }}
{{- .Values.database.existingAdminSecret }}
{{- else }}
{{- include "loom-configuration.fullname" . }}-db-admin
{{- end }}
{{- end }}

{{- define "loom-configuration.adminSecretKey" -}}
{{- default "password" .Values.database.adminSecretKey }}
{{- end }}

{{- define "loom-configuration.serviceSecretName" -}}
{{- if .Values.database.existingServiceSecret }}
{{- .Values.database.existingServiceSecret }}
{{- else }}
{{- include "loom-configuration.fullname" . }}-db-service
{{- end }}
{{- end }}

{{- define "loom-configuration.serviceSecretPasswordKey" -}}
{{- default "password" .Values.database.serviceSecretPasswordKey }}
{{- end }}

{{- define "loom-configuration.serviceSecretUsernameKey" -}}
{{- default "username" .Values.database.serviceSecretUsernameKey }}
{{- end }}

{{- define "loom-configuration.serviceSecretConnectionStringKey" -}}
{{- default "connection-string" .Values.database.serviceSecretConnectionStringKey }}
{{- end }}

{{- define "loom-configuration.layoutAdminSecretName" -}}
{{- if .Values.layoutDatabase.existingAdminSecret }}
{{- .Values.layoutDatabase.existingAdminSecret }}
{{- else }}
{{- include "loom-configuration.fullname" . }}-layout-db-admin
{{- end }}
{{- end }}

{{- define "loom-configuration.layoutServiceSecretName" -}}
{{- if .Values.layoutDatabase.existingServiceSecret }}
{{- .Values.layoutDatabase.existingServiceSecret }}
{{- else }}
{{- include "loom-configuration.fullname" . }}-layout-db-service
{{- end }}
{{- end }}
