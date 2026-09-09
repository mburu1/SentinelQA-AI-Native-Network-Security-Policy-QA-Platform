import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AiDraftDefect, AnalyzeFailureRequest, DraftDefectRequest, FailureAnalysis,
  GenerateScenariosRequest, GenerateScenariosResponse
} from '@core/models';
import { ApiBase } from './api-base.service';

@Injectable({ providedIn: 'root' })
export class AiService extends ApiBase {
  generateScenarios(req: GenerateScenariosRequest): Observable<GenerateScenariosResponse> {
    return this.http.post<GenerateScenariosResponse>(this.url('/ai/test-scenarios'), req);
  }

  analyzeFailure(req: AnalyzeFailureRequest): Observable<FailureAnalysis> {
    return this.http.post<FailureAnalysis>(this.url('/ai/analyze-failure'), req);
  }

  draftDefect(req: DraftDefectRequest): Observable<AiDraftDefect> {
    return this.http.post<AiDraftDefect>(this.url('/ai/draft-defect'), req);
  }
}