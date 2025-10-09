export interface Person {
  personId: number;
  name: string;
  currentRank: string;
  currentDutyTitle: string;
  careerStartDate: string | null;
  careerEndDate: string | null;
  astronautDetail?: AstronautDetail;
  astronautDuties?: AstronautDuty[];
}

export interface AstronautDetail {
  id: number;
  personId: number;
  currentRank: string;
  currentDutyTitle: string;
  careerStartDate: string;
  careerEndDate?: string;
  person: Person;
}

export interface AstronautDuty {
  id: number;
  personId: number;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
  dutyEndDate?: string;
  person?: Person;
}

export interface CreatePersonRequest {
  name: string;
}

export interface CreateAstronautDutyRequest {
  name: string;
  rank: string;
  dutyTitle: string;
  dutyStartDate: string;
}

export interface BaseResponse<T = any> {
  success: boolean;
  message: string;
  responseCode: number;
  data?: T;
}

export interface PersonAstronaut {
  person: Person;
  duties: AstronautDuty[];
}