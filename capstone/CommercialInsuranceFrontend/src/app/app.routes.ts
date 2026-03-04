// Implementing core module functionality and external dependencies.
import { Routes } from '@angular/router';
import { LandingPageComponent } from './pages/landing-page/landing-pagecomponent';
import { LoginComponent } from './pages/login/logincomponent';
import { AdminDashboardComponent } from './pages/admin-dashboard/admin-dashboardcomponent';
import { AdminAnalyticsComponent } from './pages/admin-dashboard/admin-analytics/admin-analyticscomponent';
import { AdminInsuranceComponent } from './pages/admin-dashboard/admin-insurance/admin-insurancecomponent';
import { AdminPoliciesComponent } from './pages/admin-dashboard/admin-policies/admin-policiescomponent';
import { AdminUsersComponent } from './pages/admin-dashboard/admin-users/admin-userscomponent';
import { AdminInquiriesComponent } from './pages/admin-dashboard/admin-inquiries/admin-inquiries';
import { PlanSelectionComponent } from './pages/insurance/plan-selection/plan-selectioncomponent';
import { BusinessProfileCreateComponent } from './pages/business-profile/business-profile-createcomponent';
import { QuoteGenerateComponent } from './pages/insurance/quote-generate/quote-generatecomponent';
import { QuoteSuccessComponent } from './pages/insurance/quote-success/quote-successcomponent';
import { PolicyPaymentComponent } from './pages/insurance/policy-payment/policy-paymentcomponent';
import { PaymentSuccessComponent } from './pages/insurance/payment-success/payment-successcomponent';
import { PaymentScheduleComponent } from './pages/insurance/payment-schedule/payment-schedulecomponent';
import { InvoiceComponent } from './pages/insurance/invoice/invoicecomponent';
import { AgentDashboardComponent } from './pages/agent-dashboard/agent-dashboardcomponent';
import { CustomerDashboardComponent } from './pages/customer-dashboard/customer-dashboardcomponent';
import { ClaimSubmissionComponent } from './pages/customer-dashboard/claim-submission/claim-submissioncomponent';
import { ClaimsOfficerDashboardComponent } from './pages/claims-officer-dashboard/claims-officer-dashboardcomponent';
import { RegisterComponent } from './pages/register/registercomponent';
import { ErrorDisplayComponent } from './pages/error/error-displaycomponent';
import { adminGuard } from './guards/authguard';
import { AboutUs } from './pages/about-us/about-uscomponent';
import { Contact } from './pages/contact/contactcomponent';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-passwordcomponent';
import { ResetPasswordComponent } from './pages/forgot-password/reset-passwordcomponent';

export const routes: Routes = [
    { path: 'error', component: ErrorDisplayComponent },
    { path: '', component: LandingPageComponent },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'forgot-password', component: ForgotPasswordComponent },
    { path: 'reset-password', component: ResetPasswordComponent },
    { path: 'about-us', component: AboutUs },
    { path: 'contact', component: Contact },
    { path: 'insurance/:typeId/plans', component: PlanSelectionComponent },
    { path: 'business-profile/create', component: BusinessProfileCreateComponent },
    { path: 'quotes/generate', component: QuoteGenerateComponent },
    { path: 'quotes/success/:id', component: QuoteSuccessComponent },
    { path: 'policies/:id/pay', component: PolicyPaymentComponent },
    { path: 'policies/:id/invoice', component: InvoiceComponent },
    { path: 'payment-success/:paymentId', component: PaymentSuccessComponent },
    { path: 'payment-schedule/:policyId', component: PaymentScheduleComponent },
    { path: 'agent/dashboard', component: AgentDashboardComponent },
    { path: 'dashboard', component: CustomerDashboardComponent },
    { path: 'claims/file/:policyId', component: ClaimSubmissionComponent },
    { path: 'claims-officer/dashboard', component: ClaimsOfficerDashboardComponent },
    {
        path: 'admin',
        component: AdminDashboardComponent,
        canActivate: [adminGuard],
        children: [
            { path: 'analytics', component: AdminAnalyticsComponent },
            { path: 'insurance', component: AdminInsuranceComponent },
            { path: 'policies', component: AdminPoliciesComponent },
            { path: 'users', component: AdminUsersComponent },
            { path: 'inquiries', component: AdminInquiriesComponent },
            { path: '', redirectTo: 'analytics', pathMatch: 'full' }
        ]
    },
    { path: '**', component: ErrorDisplayComponent, data: { status: '404' } }
];


