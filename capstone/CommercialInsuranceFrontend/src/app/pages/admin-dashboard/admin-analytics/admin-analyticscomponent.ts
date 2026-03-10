// Displays system analytics charts: policy trends, claim resolution stats, premium revenue, and user growth pulled from AnalyticsService.
import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgApexchartsModule } from 'ng-apexcharts';
import {
    ApexAxisChartSeries,
    ApexChart,
    ApexXAxis,
    ApexDataLabels,
    ApexTitleSubtitle,
    ApexNonAxisChartSeries,
    ApexPlotOptions,
    ApexTooltip,
    ApexYAxis,
    ApexFill,
    ApexLegend,
    ApexGrid,
    ApexStroke
} from 'ng-apexcharts';

import { AdminService, AdminDashboardDto } from '../../../services/adminservice';
import { AnalyticsService, ClaimsAnalyticsDto, RevenueAnalyticsDto } from '../../../services/analyticsservice';
import { forkJoin } from 'rxjs';

export type ChartOptions = {
    // State and data property: series
    series: ApexAxisChartSeries | ApexNonAxisChartSeries;
    // State and data property: chart
    chart: ApexChart;
    // State and data property: xaxis
    xaxis: ApexXAxis;
    // State and data property: yaxis
    yaxis: ApexYAxis;
    // State and data property: dataLabels
    dataLabels: ApexDataLabels;
    // State and data property: plotOptions
    plotOptions: ApexPlotOptions;
    // State and data property: title
    title: ApexTitleSubtitle;
    // State and data property: labels
    labels: string[];
    // State and data property: legend
    legend: ApexLegend;
    // State and data property: colors
    colors: string[];
    // State and data property: stroke
    stroke: ApexStroke;
    // State and data property: fill
    fill: ApexFill;
    // State and data property: tooltip
    tooltip: ApexTooltip;
    // State and data property: grid
    grid: ApexGrid;
};

@Component({
    selector: 'app-admin-analytics',
    styleUrl: './admin-analyticscomponent.css',
    standalone: true,
    imports: [CommonModule, NgApexchartsModule],
    templateUrl: './admin-analyticscomponent.html'
})
export class AdminAnalyticsComponent implements OnInit {
    private adminService = inject(AdminService);
    private analyticsService = inject(AnalyticsService);

    // State and data property: stats
    stats = signal<AdminDashboardDto | null>(null);
    // State and data property: loading
    loading = signal(true);
    // State and data property: error
    error = signal('');

    // Revenue Toggle
    revenuePeriod = signal<'monthly' | 'yearly'>('monthly');

    // Chart Data
    agentRadialOptions: Partial<ChartOptions> | null = null;
    // State and data property: claimsDonutOptions
    claimsDonutOptions: Partial<ChartOptions> | null = null;
    // State and data property: settlementsAreaOptions
    settlementsAreaOptions: Partial<ChartOptions> | null = null;
    // State and data property: revenueAreaOptions
    revenueAreaOptions: Partial<ChartOptions> | null = null;

    // State and data property: revenueData
    revenueData = signal<RevenueAnalyticsDto | null>(null);
    // Lifecycle hook: Initialization phase where initial data is loaded from services

    ngOnInit() {
        this.loadDashboardData();
    }

    // Retrieves and populates required data for loadDashboardData
    loadDashboardData() {
        this.loading.set(true);

        forkJoin({
            stats: this.adminService.getDashboardStats(),
            agentPerf: this.analyticsService.getAgentPerformance(),
            claimsPerf: this.analyticsService.getClaimsPerformance(),
            revenuePerf: this.analyticsService.getRevenueAnalytics(this.revenuePeriod())
        }).subscribe({
            next: (data) => {
                this.stats.set(data.stats);
                this.revenueData.set(data.revenuePerf);
                this.buildAgentRadialChart(data.agentPerf);
                this.buildClaimsCharts(data.claimsPerf);
                this.buildRevenueChart(data.revenuePerf);
                this.loading.set(false);
            },
            error: (err) => {
                console.error(err);
                this.error.set('Failed to load dashboard statistics.');
                this.loading.set(false);
            }
        });
    }

    // Toggles visibility or state flag for toggleRevenuePeriod
    toggleRevenuePeriod(period: 'monthly' | 'yearly') {
        this.revenuePeriod.set(period);
        this.analyticsService.getRevenueAnalytics(period).subscribe({
            next: (res) => {
                this.revenueData.set(res);
                this.buildRevenueChart(res);
            }
        });
    }

    //  Radial Bar: Agent Acquisition Force 
    private buildAgentRadialChart(data: any[]) {
        if (!data || data.length === 0) return;

        const top5 = data.slice(0, 5);
        const max = Math.max(...top5.map((d: any) => d.totalCommission), 1);

        // Convert to percentages (0–100) relative to top performer
        const percentages = top5.map((d: any) => Math.round((d.totalCommission / max) * 100));
        const names = top5.map((d: any) => d.agentName);

        this.agentRadialOptions = {
            series: percentages,
            chart: {
                type: 'radialBar',
                height: 360,
                fontFamily: 'DM Sans, sans-serif',
                toolbar: { show: false }
            },
            labels: names,
            colors: ['#734d61', '#8c667a', '#a68094', '#4d3340', '#ccb3c0'],
            plotOptions: {
                radialBar: {
                    offsetY: 0,
                    startAngle: 0,
                    endAngle: 270,
                    hollow: {
                        margin: 5,
                        size: '30%',
                        background: 'transparent'
                    },
                    track: {
                        background: '#f8f4f6',
                        strokeWidth: '97%',
                        margin: 5
                    },
                    dataLabels: {
                        name: {
                            fontSize: '12px',
                            fontWeight: '700',
                            color: '#907E80'
                        },
                        value: {
                            fontSize: '14px',
                            fontWeight: '800',
                            color: '#251C1D',
                            formatter: (val: number) => val + '%'
                        },
                        total: {
                            show: true,
                            label: 'Top Agent',
                            color: '#A89799',
                            fontSize: '11px',
                            fontWeight: '700',
                            formatter: () => top5[0]?.agentName?.split(' ')[0] || ''
                        }
                    }
                }
            },
            legend: {
                show: true,
                floating: true,
                fontSize: '11px',
                fontWeight: '700',
                fontFamily: 'DM Sans, sans-serif',
                position: 'left',
                offsetX: -10,
                offsetY: 20,
                labels: { useSeriesColors: true },
                markers: { size: 0 },
                formatter: (seriesName: string, opts: any) =>
                    seriesName + ': ' + opts.w.globals.series[opts.seriesIndex] + '%',
                itemMargin: { vertical: 3 }
            }
        } as any;
    }

    //  Donut + Area: Claims Performance 
    private buildClaimsCharts(data: ClaimsAnalyticsDto) {
        if (!data) return;

        // Donut
        const rejected = data.totalClaims - data.approvedClaims;
        this.claimsDonutOptions = {
            series: [data.approvedClaims, rejected],
            chart: {
                type: 'donut',
                height: 300,
                fontFamily: 'DM Sans, sans-serif'
            },
            labels: ['Approved / Settled', 'Pending / Rejected'],
            colors: ['#734d61', '#f8f4f6'],
            legend: {
                position: 'bottom',
                fontSize: '11px',
                fontWeight: '700',
                fontFamily: 'DM Sans, sans-serif',
                labels: { colors: ['#45393A', '#A89799'] }
            },
            dataLabels: { enabled: false },
            plotOptions: {
                pie: {
                    donut: {
                        size: '72%',
                        labels: {
                            show: true,
                            name: {
                                show: true,
                                fontSize: '11px',
                                fontWeight: '700',
                                fontFamily: 'DM Sans, sans-serif',
                                color: '#A89799',
                                offsetY: -4
                            },
                            value: {
                                show: true,
                                fontSize: '28px',
                                fontWeight: '800',
                                fontFamily: 'DM Sans, sans-serif',
                                color: '#251C1D',
                                offsetY: 8
                            },
                            total: {
                                show: true,
                                label: 'Total Claims',
                                fontSize: '10px',
                                fontWeight: '700',
                                fontFamily: 'DM Sans, sans-serif',
                                color: '#A89799',
                                formatter: (w: any) =>
                                    w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0).toString()
                            }
                        }
                    }
                }
            }
        } as any;

        // Smooth area chart per officer (instead of bar)
        const officerNames = data.officerPerformances.map(o => o.officerName).slice(0, 6);
        const officerStats = data.officerPerformances.map(o => o.totalSettlements).slice(0, 6);

        this.settlementsAreaOptions = {
            series: [{ name: 'Settlements Closed', data: officerStats }],
            chart: {
                type: 'area',
                height: 280,
                fontFamily: 'DM Sans, sans-serif',
                toolbar: { show: false },
                sparkline: { enabled: false }
            },
            colors: ['#734d61'],
            stroke: { curve: 'smooth', width: 2.5 },
            fill: {
                type: 'gradient',
                gradient: {
                    shadeIntensity: 1,
                    opacityFrom: 0.35,
                    opacityTo: 0.02,
                    stops: [0, 90, 100]
                }
            },
            dataLabels: { enabled: false },
            xaxis: {
                categories: officerNames,
                labels: {
                    style: {
                        fontSize: '10px',
                        fontWeight: '700',
                        fontFamily: 'DM Sans, sans-serif',
                        colors: '#A89799'
                    }
                },
                axisBorder: { show: false },
                axisTicks: { show: false }
            },
            yaxis: {
                labels: {
                    style: {
                        fontSize: '10px',
                        fontWeight: '600',
                        fontFamily: 'DM Sans, sans-serif',
                        colors: ['#A89799']
                    }
                }
            },
            grid: {
                borderColor: '#F0ECEC',
                strokeDashArray: 4,
                xaxis: { lines: { show: false } }
            },
            tooltip: {
                theme: 'light',
                style: { fontSize: '11px', fontFamily: 'DM Sans, sans-serif' }
            }
        } as any;
    }

    //  Area: Revenue Trend 
    private buildRevenueChart(data: RevenueAnalyticsDto) {
        if (!data || !data.revenueTrend) return;

        const keys = Object.keys(data.revenueTrend).reverse();
        const vals = Object.values(data.revenueTrend).reverse().map(v => Math.round(v));

        this.revenueAreaOptions = {
            series: [{ name: 'Net Revenue (₹)', data: vals }],
            chart: {
                type: 'area',
                height: 340,
                fontFamily: 'DM Sans, sans-serif',
                toolbar: { show: false }
            },
            dataLabels: { enabled: false },
            stroke: { curve: 'smooth', width: 2.5 },
            xaxis: {
                categories: keys,
                labels: {
                    style: {
                        fontSize: '10px',
                        fontWeight: '700',
                        fontFamily: 'DM Sans, sans-serif',
                        colors: '#A89799'
                    }
                },
                axisBorder: { show: false },
                axisTicks: { show: false }
            },
            yaxis: {
                labels: {
                    formatter: (val: number) => '₹' + (val >= 1000 ? (val / 1000).toFixed(0) + 'K' : val.toString()),
                    style: {
                        fontSize: '10px',
                        fontWeight: '600',
                        fontFamily: 'DM Sans, sans-serif',
                        colors: ['#A89799']
                    }
                }
            },
            colors: ['#5c3d4e'],
            fill: {
                type: 'gradient',
                gradient: {
                    shadeIntensity: 1,
                    opacityFrom: 0.45,
                    opacityTo: 0.02,
                    stops: [0, 90, 100]
                }
            },
            grid: {
                borderColor: '#F0ECEC',
                strokeDashArray: 4,
                xaxis: { lines: { show: false } }
            },
            tooltip: {
                theme: 'light',
                style: { fontSize: '11px', fontFamily: 'DM Sans, sans-serif' },
                y: { formatter: (val: number) => '₹' + val.toLocaleString('en-IN') }
            }
        } as any;
    }
}