export interface Job {
  id: string;
  name: string;
  status: string;
  createdAt: string;
}

export interface TranslationValidation {
  isValid: boolean;
  missingSegmentIds: string[];
  duplicateSegmentIds: string[];
  unexpectedSegmentIds: string[];
}